using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolSurface
{

    /// <summary>
    /// 记录Levelone行情的数据结构
    /// </summary>
    struct levelOne
    {
        public string code;
        public double ask, bid, askv, bidv, last;
        public int time;
        //用来做深拷贝的函数。
        public levelOne(string code,int time,double last,double ask,double askv,double bid,double bidv)
        {
            this.code = code;
            this.time = time;
            this.last = last;
            this.ask = ask;
            this.askv = askv;
            this.bid = bid;
            this.bidv = bidv;
        }
    }

    /// <summary>
    /// 期权相关合约信息
    /// </summary>
    struct optionRelation
    {
        public List<string> box,timespread;
        public string future,spotStock,parity;
    }    
    /// <summary>
    /// 存储数据信息的结构体。    
    /// /// </summary>
    struct dataFormat
    {
        public string code, status;
        public long date,time;
        public double[] ask, askv, bid, bidv;
        public double mid;
        public decimal high, low, last, openInterest, turnover, volume, count;
    }


    /// <summary>
    /// 存储期权基本信息的结构体。
    /// </summary>
    struct optionFormat
    {
        public int optionCode;
        public string optionName;
        public int startDate;
        public int endDate;
        public string optionType;
        public string executeType;
        public double strike;
        public string market;
    }

    /// <summary>
    /// 存储期权数据的结构体。包含13个字段。
    /// </summary>
    struct optionDataFormat
    {
        public string code, status;
        public int[] ask, askv, bid, bidv;
        public long high, low, last, openInterest, time, turnover, volume,count;
    }

    /// <summary>
    /// 需要提取TDB数据的品种信息。
    /// </summary>
    struct TDBdataInformation
    {
        public string market;
        public int startDate;
        public int endDate;
        public string type;
    }
    /// <summary>
    /// 记录TDB连接信息的结构体。
    /// </summary>
    struct TDBsource
    {
        public string IP;
        public string port;
        public string account;
        public string password;
        public TDBsource(string IP, string port, string account, string password)
        {
            this.IP = IP;
            this.port = port;
            this.account = account;
            this.password = password;
        }
    }
}
