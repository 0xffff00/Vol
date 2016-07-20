using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolSurface
{
    /// <summary>
    /// 计算隐含波动率的类。
    /// </summary>
    class Impv
    {
        public double impv=0;
        public optionFormat option;
        public levelOne optionData,etfData;
        public double duration;
        public double r = 0.04, rf = 0.08;

        /// <summary>
        /// 计算隐含波动率及其他希腊值的类。
        /// </summary>
        /// <param name="option">期权类型</param>
        /// <param name="optionData">期权数据</param>
        /// <param name="etfData">etf数据</param>
        /// <param name="duration">到期时间</param>
        public Impv(optionFormat option,levelOne optionData,levelOne etfData,double duration)
        {
            this.option = option;
            this.optionData = optionData;
            this.etfData = etfData;
            this.duration = duration;
        }


        /// <summary>
        /// 计算隐含波动率的函数。考虑了融券成本。
        /// </summary>
        /// <returns>隐含波动率</returns>
        public double computeVol()
        {

            double etfPrice = Convert.ToDouble(etfData.last);
            double r = this.r;
            if (option.optionType=="认沽")
            {
                etfPrice = Convert.ToDouble(etfData.last) * Math.Exp(-rf * duration / 252.0);
                r = 0.02;
            }
          //  impv = sigma(etfPrice, ComputeMidPrice(optionData.ask,optionData.bid), option.strike, duration, r,option.optionType);
            impv = approximateSigma(etfPrice, ComputeMidPrice(optionData.ask, optionData.bid), option.strike, duration, r, option.optionType);
            return impv;
        }
        public double ComputeMidPrice(double ask, double bid)
        {
            double midPrice = 0;
            if (ask > 0 && bid > 0)
            {
                midPrice = (ask + bid) / 2.0;
            }
            else if (ask > 0)
            {
                midPrice = ask;
            }
            else
            {
                midPrice = bid;
            }
            return midPrice;
        }
        public static double approximateSigma(double etfPrice, double optionPrice, double strike, double duration, double r, string optionType)
        {
            double sigma = 0.0;
            duration /= 252.0;//调整为年化的时间。
            if (optionType.Equals("认沽"))
            {
                optionPrice = optionPrice + etfPrice - strike * Math.Exp(-r * duration);
                if (strike * Math.Exp(-r * duration) - etfPrice > optionPrice)
                {
                    return 0;
                }
            }
            else
            {
                if (optionPrice < etfPrice - strike * Math.Exp(-r * duration))
                {
                    return 0;
                }
            }

            double eta = strike * Math.Exp(-r * duration) / etfPrice;
            double rho = Math.Abs(eta - 1) / Math.Pow((optionPrice / etfPrice), 2);
            double alpha = Math.Sqrt(2 * Math.PI) / (1 + eta) * (2 * optionPrice / etfPrice + eta - 1);
            double beta = Math.Cos(Math.Acos(3 * alpha / Math.Sqrt(32)) / 3);
            if (rho <= 1.4)
            {
                double radicand = 8 * beta * beta - 6 * alpha / (Math.Sqrt(2) * beta);
                if (radicand < 0)
                {
                    return sigma;
                }
                else
                {
                    sigma = Math.Sqrt(8 / duration) * beta - Math.Sqrt(radicand / duration);
                }
            }
            else
            {
                double radicand = alpha * alpha - 4 * Math.Pow(eta - 1, 2) / (1 + eta);
                if (radicand < 0)
                {
                    return sigma;
                }
                else
                {
                    sigma = (alpha + Math.Sqrt(radicand)) / (2 * Math.Sqrt(duration));
                }
            }
            return sigma;
        }

        /// <summary>
        /// 利用期权价格等参数计算隐含波动率
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="optionLastPrice">期权价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到日期</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型区分看涨还是看跌</param>
        /// <returns>返回隐含波动率</returns>
        public double sigma(double etfPrice, double optionLastPrice, double strike, double duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return sigmaOfCall(optionLastPrice, etfPrice, strike, duration / 252.0, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return sigmaOfPut(optionLastPrice, etfPrice, strike, duration / 252.0, r);
            }
            return 0;
        }

        /// <summary>
        /// 辅助函数erf(x),利用近似的方法进行计算
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回etf(x)</returns>
        private double erf(double x)
        {
            double tau = 0;
            double t = 1 / (1 + 0.5 * Math.Abs(x));
            tau = t * Math.Exp(-Math.Pow(x, 2) - 1.26551223 + 1.00002368 * t + 0.37409196 * Math.Pow(t, 2) + 0.09678418 * Math.Pow(t, 3) - 0.18628806 * Math.Pow(t, 4) + 0.27886807 * Math.Pow(t, 5) - 1.13520398 * Math.Pow(t, 6) + 1.48851587 * Math.Pow(t, 7) - 0.82215223 * Math.Pow(t, 8) + 0.17087277 * Math.Pow(t, 9));
            if (x >= 0)
            {
                return 1 - tau;
            }
            else
            {
                return tau - 1;
            }
        }

        /// <summary>
        /// 辅助函数normcdf(x)
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回normcdf(x)</returns>
        private double normcdf(double x)
        {
            return 0.5 + 0.5 * erf(x / Math.Sqrt(2));
        }

        /// <summary>
        /// 计算看涨期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看涨期权理论价格</returns>
        private double callPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((spotPrice - strike) > 0) ? (spotPrice - strike) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
        }

        /// <summary>
        /// 计算看跌期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看跌期权理论价格</returns>
        private double putPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((strike - spotPrice) > 0) ? (strike - spotPrice) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return -normcdf(-d1) * spotPrice + normcdf(-d2) * strike * Math.Exp(-r * duration);
        }

        /// <summary>
        /// 计算看涨期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        private double sigmaOfCall(double callPrice, double spotPrice, double strike, double duration, double r)
        {
            double sigma = 1, sigmaold = 1;
            //时间价值为负，隐含波动率不存在
            if (callPrice < spotPrice - strike * Math.Exp(-r * duration))
            {
                return 0;
            }
            //利用牛顿迭代计算隐含波动率
            for (int num = 0; num < 10; num++)
            {
                sigmaold = sigma;
                double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
                double d2 = d1 - sigma * Math.Sqrt(duration);
                double f_sigma = normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
                double df_sigma = spotPrice * Math.Sqrt(duration) * Math.Exp(-d1 * d1 / 2) / (Math.Sqrt(2 * Math.PI));
                sigma = sigma + (callPrice - f_sigma) / df_sigma;
                if (Math.Abs(sigma - sigmaold) < 0.0001)
                {
                    break;
                }
                //到期期限很短的时候需要额外的处理
                if (Math.Abs(sigma) > 100 && duration < 1)
                {
                    return 0;
                }
            }
            if (sigma > 3 || sigma < 0)
            {
                sigma = 0;
            }
            return sigma;
        }

        /// <summary>
        /// 计算看跌期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        private double sigmaOfPut(double putPrice, double spotPrice, double strike, double duration, double r)
        {
            double sigma = 1, sigmaold = 1;
            //时间价值为负，隐含波动率不存在
            if (strike * Math.Exp(-r * duration) - spotPrice > putPrice)
            {
                return 0;
            }
            //利用牛顿迭代计算隐含波动率
            for (int num = 0; num < 10; num++)
            {
                sigmaold = sigma;
                double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
                double d2 = d1 - sigma * Math.Sqrt(duration);
                double f_sigma = -normcdf(-d1) * spotPrice + normcdf(-d2) * strike * Math.Exp(-r * duration);
                double df_sigma = spotPrice * Math.Sqrt(duration) * Math.Exp(-d1 * d1 / 2) / (Math.Sqrt(2 * Math.PI));
                sigma = sigma + (putPrice - f_sigma) / df_sigma;
                if (Math.Abs(sigma - sigmaold) < 0.0001)
                {
                    break;
                }
                //到期期限很短的时候需要额外的处理
                if (Math.Abs(sigma) > 100 && duration < 1)
                {
                    return 0;
                }
            }
            if (sigma > 3 || sigma < 0)
            {
                sigma = 0;
            }
            return sigma;
        }

        /// <summary>
        /// 利用期权价格等参数计算隐含波动率
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="optionLastPrice">期权价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到日期</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型区分看涨还是看跌</param>
        /// <returns>返回隐含波动率</returns>
        public double sigma(double etfPrice, double optionLastPrice, double strike, int duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return sigmaOfCall(optionLastPrice, etfPrice, strike, ((double)duration) / 252.0, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return sigmaOfPut(optionLastPrice, etfPrice, strike, ((double)duration) / 252.0, r);
            }
            return 0;
        }

        /// <summary>
        /// 根据隐含波动率计算期权价格
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="sigma">隐含波动率</param>
        /// <param name="strike">期权行权价格</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型看涨还是看跌</param>
        /// <returns>返回期权理论价格</returns>
        public double optionLastPrice(double etfPrice, double sigma, double strike, int duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return callPrice(etfPrice, strike, sigma, ((double)duration) / 252.0, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return putPrice(etfPrice, strike, sigma, ((double)duration) / 252.0, r);
            }
            return 0.0;
        }

    }
}
