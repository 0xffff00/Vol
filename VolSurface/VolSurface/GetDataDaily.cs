using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace VolSurface
{
    class GetDataDaily
    {
        /// <summary>
        /// 指定的特定日期。
        /// </summary>
        public int date;
        /// <summary>
        /// 指定日期对应的期货合约代码。
        /// </summary>
        public string[] IHList;
        /// <summary>
        /// 指定日期对应的期权合约代码。
        /// </summary>
        public string[] optionList;
        /// <summary>
        /// 记录指定日期的期权期货及50ETF的一档行情数据。
        /// </summary>
        public Dictionary<string, levelOne[]> dataDaily = new Dictionary<string, levelOne[]>();       
        /// <summary>
        /// 构造函数。获取某特定日期的数据
        /// </summary>
        /// <param name="date">指定日期</param>
        public GetDataDaily(int date,string[] IHList,string[] optionList)
        {
            this.date = date;
            this.IHList = IHList;
            this.optionList = optionList;
            GetData();
        }

        /// <summary>
        /// 从数据库获取数据的主要函数。
        /// </summary>
        /// <param name="date">指定日期</param>
        public void GetData()
        {
            Get50ETFData();
            if (IHList!=null)
            {
                foreach(var item in IHList)
                {
                    GetIHData(item);
                }
            }
            if (optionList!=null)
            {
                foreach (var item in optionList)
                {
                    GetOptionData(item);
                }
            }
        }

        /// <summary>
        /// 获取50ETF期权的数据
        /// </summary>
        public void GetOptionData(string optionCode)
        {
            if (optionCode=="")
            {
                return;
            }
            string connectString = "server=192.168.38.217;database=" + GetDataBaseName(date) + ";uid =sa;pwd=maoheng0;";
            string[] str = optionCode.Split('.');
            DataTable dt = new DataApplication(GetDataBaseName(date), connectString).GetDataTable("MarketData_" + str[0] + "_SH", date);
            levelOne[] option = DataTable2Array(optionCode, dt);
            dataDaily.Add(optionCode, option);
        }

        /// <summary>
        /// 获取50ETF的数据
        /// </summary>
        public void GetIHData(string code)
        {
            if (code=="")
            {
                return;
            }
            string connectString = "server=192.168.38.216;database=" + GetDataBaseName(date) + ";uid =sa;pwd=666666;";
            string[] str = code.Split('.');
            DataTable dt = new DataApplication(GetDataBaseName(date), connectString).GetDataTable("MarketData_"+str[0]+"_CFE", date);
            levelOne[] IH = DataTable2Array(code, dt);
            dataDaily.Add(code, IH);
        }

        /// <summary>
        /// 获取50ETF的数据
        /// </summary>
        public void Get50ETFData()
        {
            string connectString = "server=192.168.38.209;database="+GetDataBaseName(date)+";uid =sa;pwd=280514;";
            DataTable dt = new DataApplication(GetDataBaseName(date), connectString).GetDataTable("MarketData_510050_SH", date);
            levelOne[] etf = DataTable2Array("510050.SH",dt);
            dataDaily.Add("510050.SH", etf);
        }

        /// <summary>
        /// 根据日期获取数据库名称。
        /// </summary>
        /// <param name="date">指定日期</param>
        /// <returns></returns>
        public string GetDataBaseName(int date)
        {
            return "TradeMarket" + (date / 100).ToString();
        }

        /// <summary>
        /// 将datatable转化为数组格式
        /// </summary>
        /// <param name="str">交易代码</param>
        /// <param name="dt">数据</param>
        /// <returns>数组形式数据</returns>
        public levelOne[] DataTable2Array(string str,DataTable dt)
        {
            levelOne[] dtArr = new levelOne[28802];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                levelOne data0 = new levelOne();
                data0.code = str;
                int time = Convert.ToInt32(dt.Rows[i]["ttime"]);
                int index = TradeDays.TimeToIndex(time);
                if (index<0 || index>28801)
                {
                    continue;
                }
                data0.time = time;
                data0.last = Convert.ToDouble(dt.Rows[i]["cp"]);
                data0.ask = Convert.ToDouble(dt.Rows[i]["S1"]);
                data0.askv = Convert.ToDouble(dt.Rows[i]["SV1"]);
                data0.bid = Convert.ToDouble(dt.Rows[i]["B1"]);
                data0.bidv = Convert.ToDouble(dt.Rows[i]["BV1"]);
                dtArr[index] = data0;
            }
            for (int i = 1; i < 28802; i++)
            {
                if (dtArr[i].last==0)
                {
                    dtArr[i] = new levelOne(dtArr[i-1].code,dtArr[i-1].time,dtArr[i-1].last,dtArr[i-1].ask,dtArr[i-1].askv,dtArr[i-1].bid,dtArr[i-1].bidv);
                }
            }
            return dtArr;
        }

    }
}
