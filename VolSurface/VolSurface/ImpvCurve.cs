using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Collections;

namespace VolSurface
{
    /// <summary>
    /// 获取波动率曲线的类。
    /// </summary>
    class ImpvCurve
    {
        /// <summary>
        /// 按照mid价格计算的认购期权的波动率曲面
        /// </summary>
        /// SortedDictionary<行权价,<到期时间,隐含波动率>>
        public SortedDictionary<double, SortedDictionary<double,double>> ImpvCurveOfCallMid = new SortedDictionary<double,SortedDictionary<double,double>>();

        /// <summary>
        /// 按照mid价格计算的认沽期权的波动率曲面
        /// </summary>
        public SortedDictionary<double, SortedDictionary<double, double>> ImpvCurveOfPutMid = new SortedDictionary<double, SortedDictionary<double, double>>();

        /// <summary>
        /// 指定日期。
        /// </summary>
        public int date;

        /// <summary>
        /// 指定的时刻。
        /// </summary>
        public int time;

        /// <summary>
        /// 给定交易日日期。
        /// </summary>
        public TradeDays myTradeDays = new TradeDays(20150209, 20160630);
        /// <summary>
        /// 指定日期对应的期货合约代码。
        /// </summary>
        public string[] IHList;
        /// <summary>
        /// 指定日期对应的期权合约代码。
        /// </summary>
        public string[] optionList;

        public int[] optionListInt;

        /// <summary>
        /// 构造函数。给定特定的日期，分析当日对应的股指期货以及期权的合约代码。
        /// </summary>
        /// <param name="date">给定日期</param>
        /// <param name="index">给定时间下标</param>
        public ImpvCurve(int date,int[] time)
        {
            this.date = date;
            ComputeVol(date, time);
        }

        public void ComputeVol(int date,int[] time)
        {
            
            if (date < 20150416)
            {
                IHList = null;//2015年4月16之前IH不存在。
            }
            if (date < 20150209)
            {
                optionList = null;//2015年2月9日之前50ETF期权不存在。
            }
            
            OptionInformation myOption = new OptionInformation(date);
            optionList = myOption.GetOptionCodeByDate(date);
            optionListInt = myOption.GetOptionNameByDate(date);
            GetDataDaily myData = new GetDataDaily(date, IHList, optionList);
            foreach (var time0 in time)
            {
                this.time = time0;
                int index = TradeDays.TimeToIndex(time0);
                ImpvCurveOfCallMid = new SortedDictionary<double, SortedDictionary<double, double>>();
                ImpvCurveOfPutMid = new SortedDictionary<double, SortedDictionary<double, double>>();
                ComputeVolDaily(myData, index);
                string CsvName = "VolatilitySurfaceCallMid" + date.ToString() + time0.ToString()+".csv";
                SaveVolToCsv(CsvName, "call", "mid", ImpvCurveOfCallMid);
                CsvName = "VolatilitySurfacePutMid" + date.ToString() +time0.ToString()+ ".csv";
                SaveVolToCsv(CsvName, "put", "mid", ImpvCurveOfPutMid);
            }
        }
        public void SaveVolToCsv(string path,string optionType,string priceType,SortedDictionary<double, SortedDictionary<double, double>> curve)
        {
                        FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs;
            fs = new FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("日期,时间,期权类型,价格类型,行权价,到期时间,隐含波动率");
            foreach (var item in curve)
            {
                string strike = item.Key.ToString();
                foreach (var item2 in item.Value)
                {
                    string duration = item2.Key.ToString();
                    string vol = item2.Value.ToString();
                    sw.WriteLine(date.ToString() + "," + time.ToString() + "," + optionType + "," + priceType + "," + strike + "," + duration + "," + vol);
                }
            }
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 计算隐含波动率
        /// </summary>
        /// <param name="myData">数据</param>
        /// <param name="index">时间下标</param>
        public void ComputeVolDaily(GetDataDaily myData,int index)
        {

            levelOne etfData = myData.dataDaily["510050.SH"][index];
            foreach (int code in optionListInt)
            {
                string codeStr = code.ToString() + ".SH";
                optionFormat option = OptionInformation.myOptionList[code];
                levelOne optionData = myData.dataDaily[codeStr][index];
                double duration = TradeDays.GetTimeSpan(date, option.endDate);
                Impv vol = new Impv(option, optionData, etfData, duration);
                double sigma=vol.computeVol();
                GetCurve(option.strike, option.optionType, duration, sigma);
            }
        }



        /// <summary>
        /// 将计算好的隐含波动率存储起来。
        /// </summary>
        /// <param name="strike">行权价</param>
        /// <param name="type">期权类型</param>
        /// <param name="duration">到期时间</param>
        /// <param name="sigma">隐含波动率</param>
        public void GetCurve(double strike,string type,double duration,double sigma)
        {
            if (type=="认购")
            {
                if (ImpvCurveOfCallMid.ContainsKey(strike))
                {
                    SortedDictionary<double, double> curve = ImpvCurveOfCallMid[strike];
                    if (curve.ContainsKey(duration))
                    {
                        ImpvCurveOfCallMid[strike][duration] = sigma;
                    }
                    else
                    {
                        ImpvCurveOfCallMid[strike].Add(duration, sigma);
                    }
                }
                else
                {
                    SortedDictionary<double, double> curve = new SortedDictionary<double, double>();
                    curve.Add(duration, sigma);
                    ImpvCurveOfCallMid.Add(strike, curve);
                }
            }
            else
            {
                if (ImpvCurveOfPutMid.ContainsKey(strike))
                {
                    SortedDictionary<double, double> curve = ImpvCurveOfPutMid[strike];
                    if (curve.ContainsKey(duration))
                    {
                        ImpvCurveOfPutMid[strike][duration] = sigma;
                    }
                    else
                    {
                        ImpvCurveOfPutMid[strike].Add(duration, sigma);
                    }
                }
                else
                {
                    SortedDictionary<double, double> curve = new SortedDictionary<double, double>();
                    curve.Add(duration, sigma);
                    ImpvCurveOfPutMid.Add(strike, curve);
                }
            }
        }
    }
}
