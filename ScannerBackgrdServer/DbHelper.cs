
/************************************* 修改记录 ******************************************
  
    一、添加数据库的各种接口
                            2018-04-23
 
 ***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ScannerBackgrdServer.Common;

namespace ScannerBackgrdServer
{
    #region 类型定义

    /// <summary>
    /// 域表的各个字段
    /// </summary>
    public struct strDomian
    {
        public int id;                //主键ID
        public string name;           //节点的名称
        public int parentId;          //节点的父亲ID
        public string nameFullPath;   //节点的名称全路径
        public int isStation;         //标识是否为站点
        public string des;            //描述
    };


    /// <summary>
    /// 设备表的各个字段
    /// </summary>
    public struct strDevice
    {
        public int id;               //设备id
        public devMode devMode;      //设备mode
        public string name;          //设备名称
        public string sn;            //SN，GSM或第三方设备可能没有该字段
        public string ipAddr;        //IP地址
        public string port;          //端口号
        public string netmask;       //掩码
        public string mode;          //设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等
        public string online;        //上下线标识，0：下线；1：上线
        public string lastOnline;    //最后的上线时间
        public string isActive;      //标识该设备是否生效，0：无效；1：生效
        public string innerType;     //用于软件内部处理
        public string affDomainId;   //标识设备的从属于那个域，FK
                       
        //以下字段不保存在库中
        public string wSelfStudy;      //0:正常状态，1:自学习状态，2018-07-20
        public string command;         //0:停止 1:开机执行 2:立即执行
        public string txpower;         //Relative to maximum output power
        public string duration;        //白名单自学习时长,单位秒
        public string clearWhiteList;  //0：否   1：是
    };


    /// <summary>
    /// AP状态表的各个字段
    /// </summary>
    public struct strApStatus
    {
        public string SCTP;          //SCTP连接状态 ：1,正常；0，不正常
        public string S1;            //S1连接状态   ：1,正常；0，不正常
        public string GPS;           //GPS连接状态  ：1,正常；0，不正常
        public string CELL;          //CELL状态     :1,正常；0，不正常
        public string SYNC;          //同步状态     ：1,正常；0，不正常
        public string LICENSE;       //LICENSE状态 ：1,正常；0，不正常
        public string RADIO;         //射频状态     ：1,正常；0，不正常
        public string time;          //时间戳
    };

    /// <summary>
    /// AP通用参数表的各个字段
    /// </summary>
    public struct strApGenPara
    {
        public string mode;              //制式：GSM,TD-SCDMA,WCDMA,LTE-TDD,LTE-FDD
        public string primaryplmn;       //主plmn
        public string earfcndl;          //工作上行频点
        public string earfcnul;          //工作下行频点
        public string cellid;            //cellid, 2018-06-26
        public string pci;               //工作pci
        public string bandwidth;         //工作带宽
        public string tac;               //TAC
        public string txpower;           //功率衰减
        public string periodtac;         //TAC变化周期
        public string manualfreq;        //选频方式 0：自动选频 1：手动选频
        public string bootMode;          //设备启动方式 0：半自动 1：全自动
        public string Earfcnlist;        //频点列表，如：38950,39150
        public string Bandoffset;        //频偏":"39,70000;38,10000
        public string NTP;               //NTP服务器ip
        public string ntppri;            //Ntp的优先级
        public string source;            //同步源（0：GPS ； 1：CNM ； 2：no sync）
        public string ManualEnable;      //是否设定手动同步源
        public string ManualEarfcn;      //手动设置同步频点
        public string ManualPci;         //手动设置同步pci
        public string ManualBw;          //手动设置同步带宽
        public string gpsConfig;         //GPS配置，0表示NOGPS，1表示GPS
      
        public string otherplmn;         //多PLMN选项，多个之间用逗号隔开
        public string periodFreq;        //{周期:freq1,freq2,freq3}
        public string res1;              //保留字段1
        public string res2;              //保留字段2
        public string res3;              //保留字段3 

        public string activeTime1Start;  //生效时间1的起始时间
        public string activeTime1Ended;  //生效时间1的结束时间
        public string activeTime2Start;  //生效时间2的起始时间
        public string activeTime2Ended;  //生效时间2的结束时间
        public string activeTime3Start;  //生效时间3的起始时间，有的话就添加该项
        public string activeTime3Ended;  //生效时间3的结束时间，有的话就添加该项
        public string activeTime4Start;  //生效时间4的起始时间，有的话就添加该项
        public string activeTime4Ended;  //生效时间4的结束时间，有的话就添加该项
        public string time;              //更新时间戳
    };


    /// <summary>
    /// 捕号记录
    /// </summary>
    public struct strCapture
    {        
        public string imsi;          //IMSI号
        public string imei;          //IMEI号
        public bwType bwFlag;        //名单标识
        public string isdn;          //手机号码号段
        public string bsPwr;         //手机上报的基站功率
        public string tmsi;          //手机TMSI号
        public string time;          //感知时间
        public string sn;            //SN号
        public string affDeviceId;   //所属设备ID

        public string name;

        //2018-07-16，for GSM
        public string regType;        //更新类型
        public string queryResult;    //查询结果
        public string localLAC;       //本地LAC
        public string sourceLAC;      //原LAC
    };


    /// <summary>
    /// bwlist表的各个字段
    /// </summary>
    public struct strBwList
    {
        public string imsi;              //IMSI号
        public string imei;              //IMEI号
        public bwType bwFlag;            //名单标识
        public string rbStart;           //起始RB
        public string rbEnd;             //结束RB
        public string time;              //设置时间
        public string des;               //描述
        public string linkFlag;          //链接标识   
        public string affDeviceId;       //所属设备ID
        public string affDomainId;       //所属域ID       
    };

    /// <summary>
    /// 查询捕号记录
    /// </summary>
    public struct strBwQuery
    {    
        public string imsi;         //IMSI号
        public string imei;         //IMEI号       
        public bwType bwFlag;       //名单标识
        public string timeStart;    //开始时间
        public string timeEnded;    //结束时间    
        public string des;
    };


    public struct strBwListQueryInfo
    {       
        public string bwListApplyTo;        
        public string deviceFullPathName;    
        public string domainFullPathName;
        public int totalRecords;
        public int totalPages;
        public int pageSize;
        public DataTable dt;
    };

    public struct strCaptureQueryInfo
    {        
        public int totalRecords;
        public int totalPages;
        public int pageSize;
        public DataTable dt;
    };


    /// <summary>
    /// 查询捕号记录
    /// </summary>
    public struct strCaptureQuery
    {
        /// <summary>
        /// 当affDeviceId为-1时，表示搜索所有的设备
        /// </summary>
        public int affDeviceId;     //所属设备ID   

        public List<int> listAffDeviceId;

        public string imsi;         //IMSI号
        public string imei;         //IMEI号       
        public bwType bwFlag;       //名单标识
        public string timeStart;    //开始时间
        public string timeEnded;    //结束时间
        public int RmDupFlag;       //是否对设备名称和SN去重标志，0:不去重，1:去重
        public UInt32 topCount;     //返回最前面的记录数，0:所有，非0:指定的记录数          
    };

    /// <summary>
    /// gsm_sys_para表的各个字段
    /// </summary>
    public struct strGsmSysPara
    {
        public string paraMcc;      //移动国家码
        public string paraMnc;      //移动网号
        public string paraBsic;     //基站识别码
        public string paraLac;      //位置区号
        public string paraCellId;   //小区ID
        public string paraC2;       //C2偏移量
        public string paraPeri;     //周期性位置更新周期
        public string paraAccPwr;   //接入功率
        public string paraMsPwr;    //手机发射功率
        public string paraRejCau;   //位置更新拒绝原因
        public string bindingDevId; //绑定的设备ID
    };


    /// <summary>
    /// gsm_sys_option表的各个字段
    /// </summary>
    public struct strGsmSysOption
    {
        public string opLuSms;      //登录时发送短信
        public string opLuImei;     //登录时获取IMEI
        public string opCallEn;     //允许用户主叫
        public string opDebug;      //调试模式，上报信令
        public string opLuType;     //登录类型
        public string opSmsType;    //短信类型
        public string opRegModel;   //注册工作模式
        public string bindingDevId; //绑定的设备ID
    };

    /// <summary>
    /// gsm_rf_para表的各个字段
    /// </summary>
    public struct strGsmRfPara
    {
        public string rfEnable;          //射频使能
        public string rfFreq;            //信道号
        public string rfPwr;             //发射功率衰减值    
        public string bindingDevId;      //绑定的设备ID

        // 2018-07-23
        public string res1;              //保留字段1
        public string res2;              //保留字段2
        public string res3;              //保留字段3

        // 2018-07-18
        public string activeTime1Start;  //生效时间1的起始时间
        public string activeTime1Ended;  //生效时间1的结束时间
        public string activeTime2Start;  //生效时间2的起始时间
        public string activeTime2Ended;  //生效时间2的结束时间
        public string activeTime3Start;  //生效时间3的起始时间，有的话就添加该项
        public string activeTime3Ended;  //生效时间3的结束时间，有的话就添加该项
        public string activeTime4Start;  //生效时间4的起始时间，有的话就添加该项
        public string activeTime4Ended;  //生效时间4的结束时间，有的话就添加该项
    };

    /// <summary>
    /// gsm_msg_option表的各个字段
    /// </summary>
    public struct strGsmMsgOption
    {
        public string smsRPOA;        //短消息中心号码
        public string smsTPOA;        //短消息原叫号码
        public string smsSCTS;        //短消息发送时间
        public string smsDATA;        //短消息内容
        public string autoSend;       //是否自动发送
        public string autoFilterSMS;  //是否自动过滤短信
        public string delayTime;      //发送延时时间
        public string smsCoding;      //短信的编码格式
        public string bindingDevId;   //绑定的设备ID
    };

    public struct str_Gsm_All_Para
    {
        public int sys;                        //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
        public int hardware_id;                //硬件id

        public bool gsmSysParaFlag;
        public strGsmSysPara gsmSysPara;       //gsm_sys_para表的各个字段

        public bool gsmSysOptionFlag;
        public strGsmSysOption gsmSysOption;   //gsm_sys_option表的各个字段

        public bool gsmRfParaFlag;
        public strGsmRfPara gsmRfPara;         //gsm_rf_para表的各个字段

        public bool gsmMsgOptionFlag;
        public strGsmMsgOption gsmMsgOption;   //gsm_msg_option表的各个字段
        public List<strGsmMsgOption> listGMO;
    };

    /// <summary>
    /// redirection表的各个字段
    /// </summary>
    public struct strRedirection
    {
        public string category;         //0:white,1:black,2:other
        public string priority;         //2:2G,3:3G,4:4G,Others:noredirect
        public string GeranRedirect;    //0:disable;1:enable
        public string arfcn;            //2G frequency    
        public string UtranRedirect;    //0:disable;1:enable
        public string uarfcn;           //3G frequency
        public string EutranRedirect;   //0:disable;1:enable
        public string earfcn;           //4G frequency
        public string RejectMethod;     //1,2,0xFF,0x10-0xFE
        public string additionalFreq;   //uarfcn,uarfcn;不超过7个freq，超过7个freq的默认丢弃
    };


    /// <summary>
    /// gc_nb_cell表的各个字段
    /// </summary>
    public struct strGcNbCell
    {
        public string bGCId;         //小区ID。CDMA没有小区ID，高WORD是SID，低WORD是NID
        public string bPLMNId;       //邻小区PLMN标志
        public string cRSRP;         //信号功率
        public string wTac;          //追踪区域码
        public string wPhyCellId;    //小区ID
        public string wUARFCN;       //小区频点
        public string cRefTxPower;   //GSM制式时为C1测量值
        public string bNbCellNum;    //邻小区个数
        public string bC2;           //C2测量值（GSM）,其他制式保留
        public string bReserved1;    //只用于LTE

        public string nc_item;                 //邻小区Item;
        public List<strGcNbCellItem> listItem; //邻小区Item
    };


    /// <summary>
    /// 邻小区Item
    /// </summary>
    public struct strGcNbCellItem
    {
        public string wUarfcn;      //小区频点
        public string wPhyCellId;   //物理小区ID
        public string cRSRP;        //信号功率
        public string bReserved;    //保留
        public string cC1;          //C1测量值
        public string bC2;          //C2测量值
    };


    public struct strGcParamConfig
    {
        public string bWorkingMode;          //工作模式。1：侦码模式；3：驻留模式(GSM/CDMA支持)
        public string bC;                    //是否自动切换模式。保留
        public string wRedirectCellUarfcn;   //CDMA黑名单频点
        public string dwDateTime;            //当前时间
        public string bPLMNId;               //PLMN标志
        public string bTxPower;              //实际发射功率.设置发射功率衰减寄存器, 0输出最大功率, 每增加1, 衰减1DB
        public string bRxGain;               //接收信号衰减寄存器. 每增加1增加1DB的增益
        public string wPhyCellId;            //物理小区ID。GSM：不用；CDMA：PN
        public string wLAC;                  //追踪区域码。GSM：LAC;CDMA：REG_ZONE
        public string wUARFCN;               //小区频点。CDMA制式为BSID
        public string dwCellId;              //小区ID。CDMA制式没有小区ID，高WORD是SID，低WORD是NID
     
        public string res1;                //保留字段1
        public string res2;                //保留字段2
        public string res3;                //保留字段3

        public string bindingDevId;        //仅用于标识GSM的绑定设备id
    };

    public struct strGcCarrierMsg
    {      
        public string wARFCN1;              //工作频点1
        public string bARFCN1Mode;          //工作频点1模式。0表示扫描，1表示常开,2表示关闭
        public string bReserved1;           //保留字段1
        public string wARFCN1Duration;      //工作频点1扫描时长
        public string wARFCN1Period;        //工作频点1扫描间隔

        public string wARFCN2;              //工作频点2
        public string bARFCN2Mode;          //工作频点2模式。0表示扫描，1表示常开,2表示关闭
        public string bReserved2;           //保留字段2
        public string wARFCN2Duration;      //工作频点2扫描时长
        public string wARFCN2Period;        //工作频点2扫描间隔

        public string wARFCN3;              //工作频点3
        public string bARFCN3Mode;          //工作频点3模式。0表示扫描，1表示常开,2表示关闭
        public string bReserved3;           //保留字段3
        public string wARFCN3Duration;      //工作频点3扫描时长
        public string wARFCN3Period;        //工作频点3扫描间隔

        public string wARFCN4;              //工作频点4
        public string bARFCN4Mode;          //工作频点4模式。0表示扫描，1表示常开,2表示关闭
        public string bReserved4;           //保留字段4
        public string wARFCN4Duration;      //工作频点4扫描时长
        public string wARFCN4Period;        //工作频点4扫描间隔       

        public string bindingDevId;        //仅用于标识GSM的绑定设备id
    };

    public struct strGcMisc
    {
        public string wTraceLen;          //Trace长度
        public string cTrace;             //Trace内容

        public string bOrmType;           //主叫类型，1=呼叫号码, 2=短消息PDU, 3=寻呼测量
        public string bUeId;              //IMSI
        public string cRSRP;              //接收信号强度
        public string bUeContentLen;      //Ue主叫内容长度
        public string bUeContent;         //Ue主叫内容，可变长度，最大249字节
    
        public string bSMSOriginalNumLen; //主叫号码长度
        public string bSMSOriginalNum;    //主叫号码
        public string bSMSContentLen;     //短信内容字数，0~70
        public string bSMSContent;        //短信内容

        public string SCTP;          //SCTP连接状态 ：1,正常；0，不正常
        public string S1;            //S1连接状态   ：1,正常；0，不正常
        public string GPS;           //GPS连接状态  ：1,正常；0，不正常
        public string CELL;          //CELL状态     :1,正常；0，不正常
        public string SYNC;          //同步状态     ：1,正常；0，不正常
        public string LICENSE;       //LICENSE状态 ：1,正常；0，不正常
        public string RADIO;         //射频状态     ：1,正常；0，不正常
        public string time;          //时间戳

        // 2018-07-18
        public string activeTime1Start;  //生效时间1的起始时间
        public string activeTime1Ended;  //生效时间1的结束时间
        public string activeTime2Start;  //生效时间2的起始时间
        public string activeTime2Ended;  //生效时间2的结束时间
        public string activeTime3Start;  //生效时间3的起始时间，有的话就添加该项
        public string activeTime3Ended;  //生效时间3的结束时间，有的话就添加该项
        public string activeTime4Start;  //生效时间4的起始时间，有的话就添加该项
        public string activeTime4Ended;  //生效时间4的结束时间，有的话就添加该项

        public string bindingDevId;       //仅用于标识GSM的绑定设备id
    };

    public struct strGcImsiAction
    {
        public string bIMSI;          //IMSI号
        public string bUeActionFlag;  //目标IMSI对应的动作，1 = Reject 5 = Hold ON'

        public string res1;           //保留字段1
        public string res2;           //保留字段2
        public string res3;           //保留字段3

        public string bindingDevId;   //仅用于标识GSM的绑定设备id
    };


    /// <summary>
    /// GSM-V2和CDMA
    /// </summary>
    public struct str_GC_All_Para
    {
        public int sys;                        //系统号，0表示系统1或通道1或射频1，1表示系统2或通道2或射频2
        public int hardware_id;                //硬件id
        public string Protocol;                //"GSM"或者"CDMA"

        // 暂时不用
        public bool gcNbCellFlag;
        public List<strGcNbCell> listGcNbCell;      

        public bool gcParamConfigFlag;
        public strGcParamConfig gcParamConfig;

        public bool gcCarrierMsgFlag;
        public strGcCarrierMsg gcCarrierMsg;        

        public bool gcMiscFlag;
        public strGcMisc gcMisc;        

        public bool gcImsiActionFlag;
        public string actionType;
        public List<strGcImsiAction> listGcImsiAction;   

    };

    public enum RC    //数据库返回代码
    {
        SUCCESS = 0,  //成功

        EXIST = 1,      //记录已经存在
        NO_EXIST = -6,  //记录不存在

        NO_OPEN = -1,      //数据库尚未打开
        PAR_NULL = -2,     //参数为空
        PAR_LEN_ERR = -3,  //参数长度有误
        OP_FAIL = -4,      //数据库操作失败
        PSW_ERR = -5,      //验证失败，密码有误
        PAR_FMT_ERR = -7,  //参数格式有误

        NO_INS_DEFUSR = -8,   //不能插入默认用户engi,root
        NO_DEL_DEFUSR = -9,   //不能删除默认用户engi,root
        FAIL_NO_USR = -10,    //验证失败，用户不存在
        FAIL_NO_MATCH = -11,  //用户和密码不匹配

        NO_INS_DEFRT = -12,    //不能插入默认角色类型Engineering,SuperAdmin,Administrator,SeniorOperator,Operator
        NO_DEL_DEFRT = -13,    //不能删除默认角色类型Engineering,SuperAdmin,Administrator,SeniorOperator,Operator

        NO_INS_DEFROLE = -14,  //不能插入默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP
        NO_DEL_DEFROLE = -15,  //不能删除默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP 

        NO_EXIST_RT = -16,      //角色类型不存在
        TIME_FMT_ERR = -17,     //时间格式有误

        USR_NO_EXIST = -18,      //usrName不存在
        ROLE_NO_EXIST = -19,     //roleName不存在

        NO_ROLE_ENG_SA = -20,    //不能指定到RoleEng和RoleSA中
        ID_SET_ERR = -21,        //ID集合有误

        IS_STATION = -22,        //域ID是站点
        IS_NOT_STATION = -23,    //域ID不是站点

        NO_EXIST_PARENT = -24,    //父亲节点不存在
        GET_PARENT_FAIL = -25,    //父亲节点信息获取失败

        ID_SET_FMT_ERR = -26,    //ID集合格式有误

        MODIFIED_EXIST = -27,    //修改后的记录已经存在
        DEV_NO_EXIST = -28,      //设备不存在
        CANNOT_DEL_ROOT = -29,   //不能删除设备的根节点

        IMSI_IMEI_BOTH_NULL    = -30,   //IMSI和IMEI都为空
        IMSI_IMEI_BOTH_NOTNULL = -31,   //IMSI和IMEI都不为空

        AP_MODE_ERR = -32,           //AP的制式不对
        CAP_QUERY_INFO_ERR = -33,    //捕号查询信息有误
        TIME_ST_EN_ERR = -34,        //开始时间大于结束时间
        DOMAIN_NO_EXIST = -35,       //域不存在

        CARRY_ERR = -36,             //载波非法
    }

    /// <summary>
    /// 省信息
    /// </summary>
    public struct Province
    {
        public string provice_id;
        public string provice_name;
    };

    /// <summary>
    /// 市信息
    /// </summary>
    public struct City
    {
        public string city_id;
        public string city_name;
    };

    /// <summary>
    /// 区信息
    /// </summary>
    public struct Distract
    {
        public string county_id;
        public string county_name;
    };


    /// <summary>
    /// 黑白名单类型
    /// </summary>
    public enum bwType    
    {
        /*
         * 数据库中的枚举是从1开始算起的
         */ 
        BWTYPE_WHITE = 1,  //黑名单 
        BWTYPE_BLACK = 2,  //白名单
        BWTYPE_OTHER = 3,  //其他名单
        BWTYPE_ALL = 4     //所有
    }

    /// <summary>
    /// 设备的制式mode
    /// 软件的内部类型也统一一样定义
    /// 2018-07-30
    /// </summary>
    public enum devMode
    {
        MODE_GSM = 0,        //对应"GSM"
        MODE_GSM_V2 = 1,     //对应"GSM-V2"
        MODE_TD_SCDMA = 2,   //对应"TD-SCDMA"
        MODE_CDMA = 3,       //对应"CDMA"
        MODE_WCDMA = 4,      //对应"WCDMA "
        MODE_LTE_TDD = 5,    //对应"LTE-TDD"
        MODE_LTE_FDD = 6,    //对应"LTE-TDD"    
        MODE_UNKNOWN = 7     //"非上述定义类型"
    }

    #endregion

    public class DbHelper
    {
        #region 定义        

        private MySqlConnection myDbConn;

        private string server;
        private string database;
        private string uid;
        private string password;
        private string port;
        private bool myDbConnFlag = false;

        private Dictionary<int, string> dicRTV = new Dictionary<int, string>();

        #endregion

        #region 属性

        /// <summary>
        /// 是否已经连接上数据库的标识
        /// </summary>
        public bool MyDbConnFlag
        {
            get
            {
                return myDbConnFlag;
            }

            //set 
            //{ 
            //    myDbConnFlag = value; 
            //}
        }        

        #endregion

        #region 构造函数

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conString">连接数据库的字符串</param>
        public DbHelper(string conString)
        {
            myDbConn = new MySqlConnection(conString);
            OpenDbConn();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="server">数据库所在机器的IP地址</param>
        /// <param name="database">数据库名称</param>
        /// <param name="uid">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="port">端口</param>
        public DbHelper(string server, string database, string uid, string password, string port)
        {           
            this.server = server;
            this.uid = uid;
            this.password = password;
            this.port = port;
            this.database = database;

            string conString = "Data Source=" + server + ";" + "port=" + port + ";" + "Database=" + database + ";" + "User Id=" + uid + ";" + "Password=" + password + ";" + "CharSet=utf8";
            myDbConn = new MySqlConnection(conString);

            OpenDbConn();
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DbHelper() : this("172.17.8.130", "hsdatabase", "root", "root", "3306")
        {
        }

        #endregion

        #region 打开和关闭


        /// <summary>
        /// open connection to database
        /// </summary>
        /// <returns>
        /// true  : 成功
        /// false ：失败
        /// </returns>
        public bool OpenDbConn()
        {
            dicRTV.Add((int)RC.SUCCESS,  "成功");
            dicRTV.Add((int)RC.EXIST,    "记录已经存在");
            dicRTV.Add((int)RC.NO_EXIST, "记录不存在");

            dicRTV.Add((int)RC.NO_OPEN,     "数据库尚未打开");
            dicRTV.Add((int)RC.PAR_NULL,    "参数为空");
            dicRTV.Add((int)RC.PAR_LEN_ERR, "参数长度有误");
            dicRTV.Add((int)RC.OP_FAIL,     "数据库操作失败");
            dicRTV.Add((int)RC.PSW_ERR,     "验证失败，密码有误");
            dicRTV.Add((int)RC.PAR_FMT_ERR, "参数格式有误");

            dicRTV.Add((int)RC.NO_INS_DEFUSR, "不能插入默认用户engi,root");
            dicRTV.Add((int)RC.NO_DEL_DEFUSR, "不能删除默认用户engi,root");
            dicRTV.Add((int)RC.FAIL_NO_USR,   "验证失败，用户不存在");
            dicRTV.Add((int)RC.FAIL_NO_MATCH, "用户和密码不匹配");

            dicRTV.Add((int)RC.NO_INS_DEFRT, "不能插入默认角色类型Engineering,SuperAdmin,Administrator,SeniorOperator,Operator");
            dicRTV.Add((int)RC.NO_DEL_DEFRT, "不能删除默认角色类型Engineering,SuperAdmin,Administrator,SeniorOperator,Operator");

            dicRTV.Add((int)RC.NO_INS_DEFROLE, "不能插入默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP");
            dicRTV.Add((int)RC.NO_DEL_DEFROLE, "不能删除默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP");

            dicRTV.Add((int)RC.NO_EXIST_RT, "角色类型不存在");
            dicRTV.Add((int)RC.TIME_FMT_ERR, "时间格式有误");

            dicRTV.Add((int)RC.USR_NO_EXIST, "usrName不存在");
            dicRTV.Add((int)RC.ROLE_NO_EXIST, "roleName不存在");

            dicRTV.Add((int)RC.NO_ROLE_ENG_SA, "不能指定到RoleEng和RoleSA中");
            dicRTV.Add((int)RC.ID_SET_ERR, "ID集合有误");

            dicRTV.Add((int)RC.IS_STATION, "域ID是站点");
            dicRTV.Add((int)RC.IS_NOT_STATION, "域ID不是站点");

            dicRTV.Add((int)RC.NO_EXIST_PARENT, "父亲节点不存在");
            dicRTV.Add((int)RC.GET_PARENT_FAIL, "父亲节点信息获取失败");

            dicRTV.Add((int)RC.ID_SET_FMT_ERR, "ID集合格式有误");

            dicRTV.Add((int)RC.MODIFIED_EXIST, "修改后的记录已经存在");
            dicRTV.Add((int)RC.DEV_NO_EXIST, "设备不存在");
            dicRTV.Add((int)RC.CANNOT_DEL_ROOT, "不能删除设备的根节点");

            dicRTV.Add((int)RC.IMSI_IMEI_BOTH_NULL, "IMSI和IMEI都为空");
            dicRTV.Add((int)RC.IMSI_IMEI_BOTH_NOTNULL, "IMSI和IMEI都不为空");

            dicRTV.Add((int)RC.AP_MODE_ERR, "AP的制式不对");
            dicRTV.Add((int)RC.CAP_QUERY_INFO_ERR, "捕号查询信息有误");
            dicRTV.Add((int)RC.TIME_ST_EN_ERR, "开始时间大于结束时间");    
            dicRTV.Add((int)RC.DOMAIN_NO_EXIST, "域不存在");

            dicRTV.Add((int)RC.CARRY_ERR, "载波非法");

            try
            {
                myDbConn.Open();
                myDbConnFlag = true;
                return true;
            }
            catch (MySqlException e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return false;
            }
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <returns>
        /// true  : 成功
        /// false ：失败
        /// </returns>
        public bool CloseDbConn()
        {
            try
            {
                myDbConn.Close();
                myDbConnFlag = false;
                return true;
            }
            catch (MySqlException e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return false;
            }
        }

        #endregion

        #region 获取表和列的名称

        /// <summary>
        /// 获取数据库中所有的表名称
        /// </summary>
        /// <returns>
        /// 成功 ： 数据库中所有表名的列表
        /// 失败 ： null
        /// </returns>
        public List<string> Get_All_TableName()
        {
            List<string> retNameList = new List<string>();

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.INFO,"数据库尚未连接", "DB", LogCategory.I);
                return null;
            }
           
            DataTable tbName = myDbConn.GetSchema("Tables");

            if (tbName.Columns.Contains("TABLE_NAME"))
            {
                foreach (DataRow dr in tbName.Rows)
                {
                    retNameList.Add((string)dr["TABLE_NAME"]);
                }
            }

            return retNameList;
        }

        /// <summary>
        /// 获取某个表中的所有列
        /// </summary>
        /// <param name="tableName">要获取的表名称</param>
        /// <returns>
        /// 成功 ： 返回tableName中所有的列名称
        /// 失败 :  null
        /// </returns>
        public List<string> Get_All_ColumnName(string tableName)
        {
            List<string> columnName = new List<string>();

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.INFO, "数据库尚未连接", "DB", LogCategory.I);
                return null;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                Logger.Trace(LogInfoType.INFO, "tableName is null.", "DB", LogCategory.I);
                return null;
            }

            string sql = string.Format("show columns from {0};", tableName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            columnName.Add(dr[0].ToString());
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return null;
            }

            return columnName;
        }

        public DataTable GetSchema(string str)
        {
            return myDbConn.GetSchema(str);
        }

        public DataTable GetSchema(string str, string[] restri)
        {
            return myDbConn.GetSchema(str, restri);
        }

        /// <summary>
        /// 通过数据库操作返回码获取对应的字符串
        /// </summary>
        /// <param name="rtCode">数据库操作返回码</param>
        /// <returns>
        /// 成功 ： 非null
        /// 失败 ： null
        /// </returns>
        public string get_rtv_str(int rtCode)
        {
            if (dicRTV.ContainsKey(rtCode))
            {
                return dicRTV[rtCode];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 01-user操作

        /// <summary>
        /// 检查用户记录是否存在
        /// </summary>
        /// <param name="name">用户名</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int user_record_exist(string name)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from user where name = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到用户表中
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="psw">用户密码（明文），最长32字符</param>
        /// <param name="des">描述</param>
        /// <returns>
        ///   RC.NO_OPEN    ：数据库尚未打开
        ///   RC.PAR_NULL   ：参数为空
        ///   PAR_LEN_ERR   ：参数长度有误
        ///   RC.OP_FAIL    ：数据库操作失败 
        ///   RC.NO_INS_DEFUSR ：不能插入默认用户 
        ///   RC.EXIST      ：存在
        ///   RC.SUCCESS    ：成功
        /// </returns>
        public int user_record_insert(string name,string psw,string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name)  || string.IsNullOrEmpty(psw) || string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64 || psw.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }           

            if ( (name == "root") || (name == "engi"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_INS_DEFUSR], "DB", LogCategory.I);
                return (int)RC.NO_INS_DEFUSR;
            }

            //检查用户是否存在
            if ((int)RC.EXIST == user_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into user values(NULL,'{0}','MD5({1})','{2}',now())", name,psw,des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在用户表中删除指定的用户 
        /// 同时，删除用户-角色和用户-域中相应的记录
        /// </summary>
        /// <param name="name">用户名</param>
        /// <returns>
        ///   RC.NO_OPEN    ：数据库尚未打开
        ///   RC.PAR_NULL   ：参数为空
        ///   RC.PAR_LEN_ERR：参数长度有误
        ///   RC.OP_FAIL    ：数据库操作失败 
        ///   RC.NO_DEL_DEF ：不能删除默认用户 
        ///   RC.NO_EXIST   ：记录不存在
        ///   RC.SUCCESS    ：成功
        /// </returns>
        public int user_record_delete(string name)
        {             
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }           

            if ( (name == "root") || (name == "engi"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_DEL_DEFUSR], "DB", LogCategory.I);
                return (int)RC.NO_DEL_DEFUSR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //删除user中相应的记录
            string sql = string.Format("delete from user where name = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }


            //删除userrole中相应的记录
            sql = string.Format("delete from userrole where usrName = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }


            //删除userdomain中相应的记录
            sql = string.Format("delete from userdomain where usrName = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在用户表中验证指定的用户和密码是否匹配
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="psw">用户密码</param>
        /// <returns>
        ///   RC.NO_OPEN    ：数据库尚未打开
        ///   RC.PAR_NULL   ：参数为空
        ///   RC.PAR_LEN_ERR：参数长度有误
        ///   RC.OP_FAIL    ：数据库操作失败 
        ///   RC.NO_EXIST   ：记录不存在
        ///   RC.PSW_ERR    ：验证失败，密码有误
        ///   RC.SUCCESS    ：成功
        /// </returns>
        public int user_record_check(string name, string psw)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(psw))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64 || psw.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("select count(*) from user where name = '{0}' and psw = 'MD5({1})'", name, psw);
            try
            {               
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }               
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.SUCCESS;
            }
            else
            {
                return (int)RC.PSW_ERR;
            }
        }

        /// <summary>
        /// 在用户信息表中修改用户的密码 
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="oldPsw">用户的老密码</param>
        /// <param name="newPsw">用户的新密码</param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   RC.PAR_LEN_ERR   ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败 
        ///   RC.NO_EXIST      ：记录不存在
        ///   RC.FAIL_NO_MATCH ：用户和密码不匹配
        ///   RC.SUCCESS       ：成功        
        /// </returns>
        public int user_record_update(string name, string oldPsw, string newPsw)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(oldPsw) || string.IsNullOrEmpty(newPsw))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (oldPsw.Length > 32 || newPsw.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("select count(*) from user where name = '{0}' and psw = 'MD5({1})'", name, oldPsw);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            //用户和老密码不匹配 
            if (cnt <= 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.FAIL_NO_MATCH], "DB", LogCategory.I);
                return (int)RC.FAIL_NO_MATCH;
            }

            sql = string.Format("update user set psw = 'MD5({0})' ,operTime = now() where name = '{1}'", newPsw, name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取用户表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：usrId,name,des,operTime
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int user_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("user");

            DataColumn column0 = new DataColumn("usrId", System.Type.GetType("System.UInt32"));
            DataColumn column1 = new DataColumn("name", System.Type.GetType("System.String"));            
            DataColumn column2 = new DataColumn("des", System.Type.GetType("System.String"));           
            DataColumn column3 = new DataColumn("operTime", System.Type.GetType("System.String"));
           
            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            string sql = string.Format("select usrId,name,des,operTime from user");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["usrId"] = Convert.ToUInt32(dr["usrId"]);

                            if (!string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                row["name"] = dr["name"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }

                            if (!string.IsNullOrEmpty(dr["operTime"].ToString()))
                            {
                                row["operTime"] = dr["operTime"].ToString();
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 02-roletype操作

        /// <summary>
        /// 检查角色类型记录是否存在
        /// </summary>
        /// <param name="roleType"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int roletype_record_exist(string roleType)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(roleType))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (roleType.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from roletype where roleType = '{0}'", roleType);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到角色类型表中
        /// </summary>
        /// <param name="roleType">角色类型</param>
        /// <param name="des">描述</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败
        ///   RC.EXIST       ：记录已经存在
        ///   RC.NO_INS_DEFRT：不能插入默认角色类型Engineering,SuperAdmin,Administrator,SeniorOperator,Operator
        ///   RC.SUCCESS     ：成功
        /// </returns>
        public int roletype_record_insert(string roleType,string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(roleType) || string.IsNullOrEmpty(des) )
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (roleType.Length > 64 || des.Length > 256)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((roleType == "Engineering")    || 
                (roleType == "SuperAdmin")     ||
                (roleType == "Administrator")  ||
                (roleType == "SeniorOperator") ||
                (roleType == "Operator"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_INS_DEFRT], "DB", LogCategory.I);
                return (int)RC.NO_INS_DEFRT;
            }

            //检查用户是否存在
            if ((int)RC.EXIST == roletype_record_exist(roleType))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into roletype values(NULL,'{0}','{1}')", roleType, des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在角色类型表中删除指定的记录 
        /// 同时删掉到roleType关联的role
        /// </summary>
        /// <param name="roleType">角色类型</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败
        ///   RC.NO_EXIST    ：记录不存在
        ///   RC.NO_DEL_DEFRT：不能删除默认角色类型Engineering,SuperAdmin,
        ///                    Administrator,SeniorOperator,Operator
        ///   RC.SUCCESS     ：成功
        /// </returns>
        public int roletype_record_delete(string roleType)
        {          
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(roleType))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (roleType.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((roleType == "Engineering")    ||
                (roleType == "SuperAdmin")     ||
                (roleType == "Administrator")  ||
                (roleType == "SeniorOperator") ||
                (roleType == "Operator"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_DEL_DEFRT], "DB", LogCategory.I);
                return (int)RC.NO_DEL_DEFRT;
            }
           
            //检查记录是否存在
            if ((int)RC.NO_EXIST == roletype_record_exist(roleType))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }            

            string sql = string.Format("delete from roletype where roleType = '{0}'", roleType);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            sql = string.Format("delete from role where roleType = '{0}'", roleType);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取角色类型表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,roleType,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int roletype_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("roletype");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "roleType";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);    

            string sql = string.Format("select * from roletype");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToUInt32(dr[0]);
                            row[1] = dr[1].ToString();

                            if (!string.IsNullOrEmpty(dr[2].ToString()))
                            {
                                row[2] = dr[2].ToString();
                            }
                            else
                            {
                                row[2] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 03-role操作

        /// <summary>
        /// 检查角色记录是否存在
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int role_record_exist(string name)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from role where name = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到角色表中
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roleType"></param>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   PAR_LEN_ERR      ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败
        ///   RC.EXIST         ：记录已经存在
        ///   RC.NO_INS_DEFROLE：不能插入默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP
        ///   NO_EXIST_RT      ：角色类型不存在
        ///   TIME_FMT_ERR     ：时间格式有误
        ///   RC.SUCCESS       ：成功
        /// </returns>
        public int role_record_insert(string name, string roleType, string timeStart,string timeEnd, string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name)      ||
                string.IsNullOrEmpty(roleType)  ||
                string.IsNullOrEmpty(timeStart) ||
                string.IsNullOrEmpty(timeEnd)   ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64 || des.Length > 256 || roleType.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            try
            {
                DateTime.Parse(timeStart);
                DateTime.Parse(timeEnd);
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message + dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.TIME_FMT_ERR;
            }

            
            if (string.Compare(timeStart, timeEnd) > 0 )
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.TIME_FMT_ERR;
            }

            if ((name == "RoleEng")   ||
                (name == "RoleSA")    ||
                (name == "RoleAdmin") ||
                (name == "RoleSO")    ||
                (name == "RoleOP"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_INS_DEFROLE], "DB", LogCategory.I);
                return (int)RC.NO_INS_DEFROLE;
            }   
            

            //检查角色记录是否存在
            if( (int) RC.EXIST == role_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            //检查角色类型记录是否存在
            if ((int)RC.NO_EXIST == roletype_record_exist(roleType))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST_RT], "DB", LogCategory.I);
                return (int)RC.NO_EXIST_RT;
            }

            string sql = string.Format("insert into role values(NULL,'{0}','{1}','{2}','{3}','{4}')", name,roleType, timeStart,timeEnd,des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在角色表中删除指定的记录 
        /// 同时，删除角色-权限表中相应的记录
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   PAR_LEN_ERR      ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败
        ///   RC.NO_EXIST      ：记录不存在
        ///   RC.NO_DEL_DEFROLE：不能删除默认角色RoleEng,RoleSA,RoleAdmin,RoleSO,RoleOP
        ///   RC.SUCCESS       ：成功
        /// </returns>
        public int role_record_delete(string name)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((name == "RoleEng")   ||
                (name == "RoleSA")    ||
                (name == "RoleAdmin") ||
                (name == "RoleSO")    ||
                (name == "RoleOP"))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_DEL_DEFROLE], "DB", LogCategory.I);
                return (int)RC.NO_DEL_DEFROLE;
            }

            //检查角色记录是否存在
            if ((int)RC.NO_EXIST == role_record_exist(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //删除role表中的记录
            string sql = string.Format("delete from role where name = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }


            //删除roleprivilege表中的记录
            sql = string.Format("delete from roleprivilege where roleName = '{0}'", name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取角色表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：roleId,name,roleType,timeStart,timeEnd,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int role_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("role");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "roleType";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "timeStart";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.String");
            column4.ColumnName = "timeEnd";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);

            string sql = string.Format("select * from role");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToUInt32(dr[0]);
                            row[1] = dr[1].ToString();
                            row[2] = dr[2].ToString();
                            row[3] = dr[3].ToString();
                            row[4] = dr[4].ToString();

                            if (!string.IsNullOrEmpty(dr[5].ToString()))
                            {
                                row[5] = dr[5].ToString();
                            }
                            else
                            {
                                row[5] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 04-privilege操作

        /// <summary>
        /// 检查权限记录是否存在
        /// </summary>
        /// <param name="funName">功能名称</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int privilege_record_exist(string funName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(funName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (funName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from privilege where funName = '{0}'", funName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查权限记录是否存在
        /// </summary>
        /// <param name="priId">priId</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int privilege_record_exist(int priId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }           

            string sql = string.Format("select count(*) from privilege where priId = {0}", priId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到权限表中
        /// </summary>
        /// <param name="funName"></param>
        /// <param name="aliasName"></param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   PAR_LEN_ERR      ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败
        ///   RC.EXIST         ：记录已经存在
        ///   RC.SUCCESS       ：成功
        /// </returns>
        public int privilege_record_insert(string funName, string aliasName,string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(funName)   ||
                string.IsNullOrEmpty(aliasName) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (funName.Length > 64 || des.Length > 256 || aliasName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查权限记录是否存在
            if ((int)RC.EXIST  == privilege_record_exist(funName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into privilege values(NULL,'{0}','{1}','{2}')", funName, aliasName,des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在权限表中删除指定的记录 
        /// </summary>
        /// <param name="name">功能名称</param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   PAR_LEN_ERR      ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败
        ///   RC.NO_EXIST      ：记录不存在
        ///   RC.SUCCESS       ：成功
        /// </returns>
        public int privilege_record_delete(string funName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(funName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (funName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == privilege_record_exist(funName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from privilege where funName = '{0}'", funName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取权限表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：priId,funName,aliasName,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int privilege_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("privilege");

            DataColumn column0 = new DataColumn("priId", System.Type.GetType("System.UInt32"));            
            DataColumn column1 = new DataColumn("funName", System.Type.GetType("System.String"));              
            DataColumn column2 = new DataColumn("aliasName", System.Type.GetType("System.String"));             
            DataColumn column3 = new DataColumn("des", System.Type.GetType("System.String"));
            
            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            string sql = string.Format("select * from privilege");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["priId"] = Convert.ToUInt32(dr["priId"]);

                            if (!string.IsNullOrEmpty(dr["funName"].ToString()))
                            {
                                row["funName"] = dr["funName"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["aliasName"].ToString()))
                            {
                                row["aliasName"] = dr["aliasName"].ToString();
                            }
                            else
                            {
                                row["aliasName"] = "";
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }  

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取权限表中的所有Id的集合
        /// </summary>
        /// <param name="listPriIdSet">
        /// priId的集合
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int privilege_record_priidset_get(ref List<string> listPriIdSet)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            listPriIdSet = new List<string>();

            string sql = string.Format("select priId from privilege");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                listPriIdSet.Add(dr[0].ToString());
                            }                            
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过id获取对应的功能ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="funName"></param>
        /// <returns>
        ///   RC.NO_OPEN       ：数据库尚未打开
        ///   RC.PAR_NULL      ：参数为空
        ///   PAR_LEN_ERR      ：参数长度有误
        ///   RC.OP_FAIL       ：数据库操作失败
        ///   RC.NO_EXIST      ：记录不存在
        ///   RC.SUCCESS       ：成功
        /// </returns>
        public int privilege_funname_get_by_id(string id, ref string funName)
        {
            //检查权限记录是否存在
            if ((int)RC.NO_EXIST == privilege_record_exist(int.Parse(id)))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            funName = "";
            string sql = string.Format("select funName from privilege where priId = {0}", id);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                funName = dr[0].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 05-userrole操作

        /// <summary>
        /// 检查用户角色记录是否存在
        /// </summary>
        /// <param name="usrName"></param>
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int userrole_record_exist(string usrName,string roleName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName) || string.IsNullOrEmpty(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 || roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from userrole where usrName = '{0}' and roleName = '{1}'", usrName, roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到用户角色表中,在此，一个用户只能指定到一个角色中
        /// </summary>
        /// <param name="usrName"></param>
        /// <param name="roleName"></param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   EXIST          ：记录已经存在
        ///   USR_NO_EXIST   ：usrName不存在
        ///   ROLE_NO_EXIST  ：roleName不存在
        ///   NO_ROLE_ENG_SA ：不能指定到RoleEng和RoleSA中
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int userrole_record_insert(string usrName, string roleName, string des)
        {            
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName) ||
                string.IsNullOrEmpty(roleName) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 || des.Length > 256 || roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }


            //检查用户角色记录是否存在
            if ((int)RC.EXIST == userrole_record_exist(usrName,roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.USR_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }


            //检查角色是否存在
            if ((int)RC.NO_EXIST == role_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ROLE_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.ROLE_NO_EXIST;
            }

            /*
             * 角色默认有5种：RoleEng，RoleSA，RoleAdmin，RoleSO，RoleOP，
             * 在指定用户到角色中时，是不能指定到RoleEng和RoleSA中的
             */
            if (roleName == "RoleEng" || roleName == "RoleSA")
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_ROLE_ENG_SA], "DB", LogCategory.I);
                return (int)RC.NO_ROLE_ENG_SA;
            }

            string sql = string.Format("insert into userrole values(NULL,'{0}','{1}','{2}')", usrName, roleName, des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在用户角色表中删除指定的记录 
        /// </summary>
        /// <param name="usrName"></param>
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   RC.SUCCESS     ：成功
        /// </returns>
        public int userrole_record_delete(string usrName,string roleName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName) || string.IsNullOrEmpty(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 || roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }


            //检查记录是否存在
            if ((int)RC.NO_EXIST == userrole_record_exist(usrName,roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from userrole where usrName = '{0}' and roleName = '{1}'", usrName, roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取用户角色表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：usrRoleId,usrName,roleName,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int userrole_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("userrole");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "usrRoleId";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "usrName";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "roleName";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            string sql = string.Format("select * from userrole");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToUInt32(dr[0]);

                            if (!string.IsNullOrEmpty(dr["usrName"].ToString()))
                            {
                                row["usrName"] = dr["usrName"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["roleName"].ToString()))
                            {
                                row["roleName"] = dr["roleName"].ToString();
                            }
                            else
                            {
                                row["roleName"] = "";
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过用户名称获取所属的用户角色
        /// </summary>
        /// <param name="usrName">用户名</param>
        /// <param name="roleName">用户所属的角色</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   USR_NO_EXIST   ：usrName不存在
        ///   ROLE_NO_EXIST  ：roleName不存在
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int userrole_get_by_user_name(string usrName, ref string roleName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.USR_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }

            roleName = null;
            string sql = string.Format("select roleName from userrole where usrName = '{0}'", usrName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                roleName = dr[0].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (roleName == null)
            {
                return (int)RC.ROLE_NO_EXIST;
            }
            else
            {
                return (int)RC.SUCCESS;
            }
        }

        #endregion

        #region 06-roleprivilege操作

        /// <summary>
        /// 检查角色权限记录是否存在
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int roleprivilege_record_exist(string roleName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if ( string.IsNullOrEmpty(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if ( roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from roleprivilege where roleName = '{0}'", roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return  (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查ID集合的合法性
        /// </summary>
        /// <param name="idSet">集合以逗号分隔，如下1,2,3,4,5</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        private bool check_id_set(string idSet)
        {
            if (string.IsNullOrEmpty(idSet))
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数为空", "DB", LogCategory.I);
                return false;
            }

            if (idSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数长度有误", "DB", LogCategory.I);
                return false;
            }

            int okFlag = 0;
            string[] s = idSet.Split(new char[] { ',' });

            if (s.Length <= 0)
            {
                return false;
            }
            else
            {                
                foreach (string str in s)
                {
                    try
                    {
                        UInt16.Parse(str);
                        okFlag++;
                    }
                    catch (Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                        //return false;                  
                    }
                }
            }

            if (okFlag > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查ID集合的合法性，返回分离后的list
        /// </summary>
        /// <param name="idSet">集合以逗号分隔，如下1,2,3,4,5</param>
        /// <param name="listStr">返回的list</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        public bool check_and_get_id_set(string idSet,ref List<string> listStr)
        {
            if (string.IsNullOrEmpty(idSet))
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数为空", "DB", LogCategory.I);
                return false;
            }

            if (idSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, "idSet参数长度有误", "DB", LogCategory.I);
                return false;
            }

            listStr = new List<string>();
            string[] s = idSet.Split(new char[] { ',' });

            if (s.Length <= 0)
            {
                return false;
            }
            else
            {
                foreach (string str in s)
                {
                    try
                    {
                        UInt16.Parse(str);
                        listStr.Add(str);
                    }
                    catch (Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                        //return false;
                    }
                }
            }

            if (listStr.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 在权限表的ID集合中，检查ID集合的合法性
        /// </summary>
        /// <param name="listStr">原始权限ID集合</param>
        /// <param name="listIdSetOk">校验后合法的ID集合</param>
        /// <returns>
        /// true  ： 成功
        /// false ： 失败
        /// </returns>
        private bool check_and_get_id_set_in_db(List<string> listStr,ref List<string> listIdSetOk)
        {
            if (listStr.Count == 0)
            {
                return false;
            }

            listIdSetOk = new List<string>();

            List<string> listPriIdSet = new List<string>();
            if ((int)RC.SUCCESS == privilege_record_priidset_get(ref listPriIdSet))
            {
                if (listPriIdSet.Count > 0)
                {
                    foreach (string str in listStr)
                    {
                        if (listPriIdSet.Contains(str))
                        {
                            listIdSetOk.Add(str);
                        }
                    }
                }
            }

            if (listIdSetOk.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 插入记录到角色-权限表中
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="priIdSet">权限ID集合，如下：1,2,3,4,5，每个权限用逗号隔开</param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   EXIST          ：记录已经存在
        ///   ROLE_NO_EXIST  ：roleName不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int roleprivilege_record_insert(string roleName, string priIdSet, string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(priIdSet) ||
                string.IsNullOrEmpty(roleName) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (priIdSet.Length > 1024 || des.Length > 256 || roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((int)RC.NO_EXIST == role_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ROLE_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.ROLE_NO_EXIST;
            }

            //检查权限ID集合的合法性
            if (false == check_id_set(priIdSet))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_ERR;
            }
            else
            {
                List<string> listPriIdSetOri = new List<string>();
                List<string> listPriIdSetChk = new List<string>();

                if (false == check_and_get_id_set(priIdSet, ref listPriIdSetOri))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }

                if (false == check_and_get_id_set_in_db(listPriIdSetOri, ref listPriIdSetChk))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }
                else
                {
                    priIdSet = "";
                    for(int i = 0;i < listPriIdSetChk.Count;i++)
                    {
                        if (i == (listPriIdSetChk.Count - 1))
                        {
                            priIdSet += listPriIdSetChk[i];
                        }
                        else
                        {
                            priIdSet += listPriIdSetChk[i] + ",";
                        }
                    }
                }
            }

            //检查记录是否存在
            if ((int)RC.EXIST == roleprivilege_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }            

            string sql = string.Format("insert into roleprivilege values(NULL,'{0}','{1}','{2}')",roleName,priIdSet,des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到角色-权限表中
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="priIdSet">权限ID集合，如下：1,2,3,4,5，每个权限用逗号隔开</param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int roleprivilege_record_update(string roleName, string priIdSet, string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(priIdSet) ||
                string.IsNullOrEmpty(roleName) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (priIdSet.Length > 1024 || des.Length > 256 || roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查权限ID集合的合法性
            if (false == check_id_set(priIdSet))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_ERR;
            }
            else
            {
                List<string> listPriIdSetOri = new List<string>();
                List<string> listPriIdSetChk = new List<string>();

                if (false == check_and_get_id_set(priIdSet, ref listPriIdSetOri))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }

                if (false == check_and_get_id_set_in_db(listPriIdSetOri, ref listPriIdSetChk))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }
                else
                {
                    priIdSet = "";
                    for (int i = 0; i < listPriIdSetChk.Count; i++)
                    {
                        if (i == (listPriIdSetChk.Count - 1))
                        {
                            priIdSet += listPriIdSetChk[i];
                        }
                        else
                        {
                            priIdSet += listPriIdSetChk[i] + ",";
                        }
                    }
                }
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == roleprivilege_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }
        
            string sql = string.Format("update roleprivilege set priIdSet='{0}',des='{1}' where roleName='{2}'", priIdSet,des,roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在角色-权限表中删除指定的记录 
        /// </summary>  
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int roleprivilege_record_delete(string roleName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == roleprivilege_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from roleprivilege where roleName = '{0}'", roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取角色-权限表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：rolePriId,roleName,priIdSet,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功  
        /// </returns>
        public int roleprivilege_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("roleprivilege");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "rolePriId";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "roleName";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "priIdSet";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            string sql = string.Format("select * from roleprivilege");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToUInt32(dr[0]);

                            if (!string.IsNullOrEmpty(dr["roleName"].ToString()))
                            {
                                row["roleName"] = dr["roleName"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["priIdSet"].ToString()))
                            {
                                row["priIdSet"] = dr["priIdSet"].ToString();
                            }
                            else
                            {
                                row["priIdSet"] = "";
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过角色名称获取对应的权限ID集合
        /// </summary>
        /// <param name="roleName">角色名称</param>
        /// <param name="listIdSet">权限ID集合</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   ROLE_NO_EXIST  ：roleName不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int roleprivilege_priidset_get_by_rolename(string roleName, ref List<string> listIdSet)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (roleName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((int)RC.NO_EXIST == role_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ROLE_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.ROLE_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == roleprivilege_record_exist(roleName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string priIdSet = "";
            string sql = string.Format("select priIdSet from roleprivilege where roleName = '{0}'", roleName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                priIdSet = dr[0].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            listIdSet = new List<string>();
            if (!check_and_get_id_set(priIdSet, ref listIdSet))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_FMT_ERR;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 07-domain操作

        /// <summary>
        /// 通过名称全路径获取对应记录的信息
        /// </summary>
        /// <param name="nameFullPath">全路径</param>
        /// <param name="str">成功时返回的记录信息</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：记录不存在
        ///   RC.SUCCESS  ：成功 
        /// </returns>
        public int domain_record_get_by_nameFullPath(string nameFullPath,ref strDomian str)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            str = new strDomian();

            string sql = string.Format("select * from domain where nameFullPath='{0}'", nameFullPath);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            str.id = Convert.ToInt32(dr["id"]);

                            if (!string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                str.name = dr["name"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["parentId"].ToString()))
                            {
                                str.parentId = Convert.ToInt32(dr["parentId"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dr["nameFullPath"].ToString()))
                            {
                                str.nameFullPath = dr["nameFullPath"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["isStation"].ToString()))
                            {
                                str.isStation = Convert.ToInt32(dr["isStation"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                str.des = dr["des"].ToString();
                            }
                            else
                            {
                                str.des = "";
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过名称全路径获取对应记录的信息
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="str">成功时返回的记录信息</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：记录不存在
        ///   RC.SUCCESS  ：成功 
        /// </returns>
        public int domain_record_get_by_nameFullPath(int id, ref strDomian str)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }
            
            if ((int)RC.NO_EXIST == domain_record_exist(id))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            str = new strDomian();

            string sql = string.Format("select * from domain where id = {0}", id);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            //str.id = Convert.ToInt32(dr[0]);
                            //str.name = dr[1].ToString();
                            //str.parentId = Convert.ToInt32(dr[2]);                           
                            //str.nameFullPath = dr[3].ToString();
                            //str.isStation = Convert.ToInt32(dr[4]);
                            //str.des = dr[5].ToString();

                            str.id = Convert.ToInt32(dr["id"]);

                            if (!string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                str.name = dr["name"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["parentId"].ToString()))
                            {
                                str.parentId = Convert.ToInt32(dr["parentId"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dr["nameFullPath"].ToString()))
                            {
                                str.nameFullPath = dr["nameFullPath"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["isStation"].ToString()))
                            {
                                str.isStation = Convert.ToInt32(dr["isStation"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                str.des = dr["des"].ToString();
                            }
                            else
                            {
                                str.des = "";
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过ID获取全路径
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nameFullPath"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：记录不存在
        ///   RC.SUCCESS  ：成功 
        /// </returns>
        public int domain_get_nameFullPath_by_id(string id, ref string nameFullPath)
        {
            if ((int)RC.NO_EXIST == domain_record_exist(int.Parse(id)))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            nameFullPath = "";
            string sql = string.Format("select nameFullPath from domain where id = {0}", id);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                nameFullPath = dr[0].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过全路径获取ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nameFullPath"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.NO_EXIST ：记录不存在
        ///   RC.SUCCESS  ：成功 
        /// </returns>
        public int domain_get_id_by_nameFullPath(string nameFullPath, ref int id)
        {
            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }
          
            string sql = string.Format("select id from domain where nameFullPath = '{0}'", nameFullPath);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                id = int.Parse(dr[0].ToString());
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 检查节点是否存在
        /// </summary>
        /// <param name="id">记录的id</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int domain_record_exist(int id)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from domain where id = {0}", id);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查节点是否存在
        /// </summary>
        /// <param name="nameFullPath">节点的全路径名称</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int domain_record_exist(string nameFullPath)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from domain where nameFullPath = '{0}'", nameFullPath);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查域ID是否为站点
        /// </summary>
        /// <param name="id">节点的id</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.IS_STATION     ：域ID是站点
        ///   RC.IS_NOT_STATION ：域ID不是是站点
        /// </returns>
        public int domain_record_is_station(int id)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }            

            string sql = string.Format("select count(*) from domain where id = '{0}' and isStation = {1}", id,1);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.IS_STATION;
            }
            else
            {
                return (int)RC.IS_NOT_STATION;
            }
        }

        /// <summary>
        /// 添加一个节点到域表中
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <param name="parentNameFullPath">节点的父亲全路径</param>
        /// <param name="isStation">是否为站点</param>
        /// <param name="des">描述</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.EXIST        ：记录已经存在
        ///   NO_EXIST_PARENT ：父亲节点不存在
        ///   GET_PARENT_FAIL ：父亲节点信息获取失败
        ///   RC.SUCCESS      ：成功 
        /// </returns>
        public int domain_record_insert(string name, string parentNameFullPath, int isStation,string des)
        {
            string curNameFullPath = "";

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name)  ||
                string.IsNullOrEmpty(parentNameFullPath) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64 || 
                des.Length > 256  ||
                parentNameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            curNameFullPath = string.Format("{0}.{1}", parentNameFullPath, name);


            //(1)先检查父亲节点是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(parentNameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST_PARENT], "DB", LogCategory.I);
                return (int)RC.NO_EXIST_PARENT;
            }


            //(2)再检查新增节点是否存在
            if ((int)RC.EXIST == domain_record_exist(curNameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            strDomian str = new strDomian();
            if ((int)RC.SUCCESS != domain_record_get_by_nameFullPath(parentNameFullPath,ref str))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.GET_PARENT_FAIL], "DB", LogCategory.I);
                return (int)RC.GET_PARENT_FAIL;
            }

            string sql = string.Format("insert into domain values(NULL,'{0}',{1},'{2}',{3},'{4}')", 
                name, str.id,curNameFullPath,isStation,des);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取一个节点下所有站点的id列表
        /// </summary>
        /// <param name="nameFullPath">节点的全路径名称</param>       
        /// <returns>
        ///   RC.NO_OPEN         ：数据库尚未打开
        ///   RC.PAR_NULL        ：参数为空
        ///   PAR_LEN_ERR        ：参数长度有误
        ///   RC.OP_FAIL         ：数据库操作失败 
        ///   RC.NO_EXIST        ：记录不存在        
        ///   RC.SUCCESS         ：成功
        /// </returns>
        public int domain_record_station_list_get(string nameFullPath,ref List<int> listID)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = "";
            listID = new List<int>();

            /*
             *  注意要加上分隔符".",如'{0}.%%'，否则会误操作
             */
            sql = string.Format("select id from domain where (nameFullPath like '{0}.%%' or nameFullPath = '{0}') and isStation = 1", nameFullPath);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                listID.Add(int.Parse(dr[0].ToString()));
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 删除一个节点和其下面的所有子孙节点(包括节点本身)
        /// 同时，删除其下面的所有设备
        /// </summary>
        /// <param name="nameFullPath">节点的全路径名称</param>       
        /// <returns>
        ///   RC.NO_OPEN         ：数据库尚未打开
        ///   RC.PAR_NULL        ：参数为空
        ///   PAR_LEN_ERR        ：参数长度有误
        ///   RC.OP_FAIL         ：数据库操作失败 
        ///   RC.NO_EXIST        ：记录不存在
        ///   RC.CANNOT_DEL_ROOT ：不能删除设备的根节点
        ///   RC.SUCCESS         ：成功
        /// </returns>
        public int domain_record_delete(string nameFullPath)
        {
            int rtv = -1;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //检查是否为设备的根节点
            if (nameFullPath == "设备")
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CANNOT_DEL_ROOT], "DB", LogCategory.I);
                return (int)RC.CANNOT_DEL_ROOT;
            }

            //获取该域下所有子孙站点的id列表
            List<int> listDomainId = new List<int>();
            rtv = domain_record_station_list_get(nameFullPath, ref listDomainId);
            if (rtv != (int)RC.SUCCESS)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[rtv], "DB", LogCategory.I);
                return rtv;
            }

            string sql = "";

            //先删除节点本身
            sql = string.Format("delete from domain where nameFullPath = '{0}'", nameFullPath);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            //再删除节点下面的所有子孙节点

            /*
             *  注意要加上分隔符".",如'{0}.%%'，否则会误删除
             */
            sql = string.Format("delete from domain where nameFullPath like '{0}.%%'", nameFullPath);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            List<string> listDevId = new List<string>();
            List<string> listDevName = new List<string>();

            //删除该节点下所有站点的设备
            foreach (int id in listDomainId)
            {
                rtv = device_id_name_get_by_affdomainid(id, ref listDevId, ref listDevName);
                if (rtv == (int)RC.SUCCESS)
                {
                    foreach (string devName in listDevName)
                    {
                        device_record_delete(id, devName);
                    }
                }
            }

            Dictionary<int, List<string>> dicUserDomain = new Dictionary<int, List<string>>();

            rtv = userdomain_record_entity_get(ref dicUserDomain);
            if (rtv != (int)RC.SUCCESS)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[rtv], "DB", LogCategory.I);
                return rtv;
            }

            /*
             * 2018-07-09
             * 删除引用了listDomainId中的所有用户-域
             */
            foreach (int id in listDomainId)
            {
                foreach (KeyValuePair<int, List<string>> kv in dicUserDomain)
                {
                    if (kv.Value.Contains(id.ToString()))
                    {
                        int key = kv.Key;
                        List<string> value = kv.Value;
                        string idSet = "";
                        for (int i = 0; i < value.Count; i++)
                        {
                            if (id.ToString() != value[i])
                            {
                                idSet += value[i] + ",";
                            }
                        }

                        if (idSet != "")
                        {
                            idSet = idSet.Remove(idSet.Length - 1, 1);
                        }

                        userdomain_record_update(key, idSet);
                    }
                }               
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 重命名节点的名称
        /// </summary>
        /// <param name="oldNameFullPath">修改前节点的全路径名称</param>
        /// <param name="newNameFullPath">修改后节点的全路径名称</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在(修改前的节点不存在)
        ///   RC.MODIFIED_EXIST ：记录已经存在(修改后的节点已经存在)
        ///   RC.SUCCESS        ：成功
        /// </returns>
        public int domain_record_rename(string oldNameFullPath,string newNameFullPath)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(oldNameFullPath) || string.IsNullOrEmpty(newNameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (oldNameFullPath.Length > 1024 || newNameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string newName = "";
            int j = newNameFullPath.LastIndexOf(".");
            newName = newNameFullPath.Substring(j+1);        

            //检查修改前节点是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(oldNameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST] + "(修改前的节点不存在)", "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //检查修改后的节点是否已经存在
            if ((int)RC.EXIST == domain_record_exist(newNameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.MODIFIED_EXIST], "DB", LogCategory.I);
                return (int)RC.MODIFIED_EXIST;
            }

            //重命名本节点本身
            string sql = string.Format("update domain set name = '{0}',nameFullPath = '{1}' where nameFullPath = '{2}'", newName,newNameFullPath, oldNameFullPath);           

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            //重命名本节点本身下的所有子节点           
            sql = string.Format("update domain set nameFullPath = REPLACE(nameFullPath, '{0}.', '{1}.') where nameFullPath like '%%{0}.%%'",oldNameFullPath, newNameFullPath);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新节点的描述
        /// </summary>
        /// <param name="nameFullPath">节点的全路径名称</param>
        /// <param name="newdes">要修改成什么样的描述</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在(修改前的节点不存在)       
        ///   RC.SUCCESS        ：成功
        /// </returns>
        public int domain_record_update_des(string nameFullPath, string newdes)
        {
            string des = "";

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (string.IsNullOrEmpty(newdes))
            {
                des = "";
            }
            else
            {
                des = newdes;
            }

            //检查修改前节点是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST] + "(修改前的节点不存在)", "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }
            
            //修改节点的描述
            string sql = string.Format("update domain set des = '{0}' where nameFullPath = '{1}'", des, nameFullPath);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }           

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取域表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,parentId,nameFullPath,isStation,des
        /// </param>
        /// <param name="isStationFlag">
        /// 是否只返回站点的记录
        /// 0：所有记录
        /// 1：只返回是站点的记录
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int domain_record_entity_get(ref DataTable dt,int isStationFlag)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("domain");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.Int32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.Int32");
            column2.ColumnName = "parentId";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "nameFullPath";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.Int32");
            column4.ColumnName = "isStation";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);

            string sql = "";

            if (1 == isStationFlag)
            {
                sql = string.Format("select * from domain where isStation = 1");
            }
            else
            {
                sql = string.Format("select * from domain");
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToInt32(dr["id"]);

                            if (!string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                row["name"] = dr["name"].ToString();
                            }

                            row["parentId"] = Convert.ToInt32(dr["parentId"]);

                            if (!string.IsNullOrEmpty(dr["nameFullPath"].ToString()))
                            {
                                row["nameFullPath"] = dr["nameFullPath"].ToString();
                            }

                            row["isStation"] = Convert.ToInt32(dr["isStation"]);

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取域表中的各条记录
        /// </summary>
        /// <param name="listSattionIdSet">
        /// 返回的所有站点id集合
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int domain_record_station_id_set_get(ref List<string> listSattionIdSet)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            listSattionIdSet = new List<string>();
            string sql = string.Format("select id from domain where isStation = 1");

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                listSattionIdSet.Add(dr[0].ToString());
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 用于快速通过设备的全名早点设备对应的ID
        /// 如：设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3，其中
        /// 设备.深圳.福田.中心广场.西北监控为域名，LTE-FDD-B3为名称
        /// 系统启动后或设备有更改后获取该字典到内存中
        /// string = 设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3
        /// int    = device的id
        /// </summary>
        /// <param name="dic">返回的字典</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int domain_dictionary_info_get(ref Dictionary<string, int> dic)
        {
            int rv;
            DataTable dt = new DataTable();

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dic = new Dictionary<string, int>();

            rv = domain_record_entity_get(ref dt, 1);
            if (rv != (int)RC.SUCCESS)
            {
                return (int)RC.OP_FAIL;
            }

            string domainId = "";
            string nameFullPath = "";

            List<string> listDevId = new List<string>();
            List<string> listDevName = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                domainId = dr[0].ToString();
                nameFullPath = dr[3].ToString();

                rv = device_id_name_get_by_affdomainid(int.Parse(domainId),ref listDevId,ref listDevName);
                if (rv != (int)RC.SUCCESS)
                {
                    return (int)RC.OP_FAIL;
                }
                else
                {
                    if (listDevName.Count > 0)
                    {                    
                        for (int i = 0; i < listDevName.Count; i++)
                        {
                            dic.Add(nameFullPath + "." + listDevName[i],int.Parse(listDevId[i]));
                        }
                    }
                }
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过mode的字符串获取对应的mode类型
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public devMode get_device_mode(string modeStr)
        {
            devMode devMode = devMode.MODE_UNKNOWN;
            if (string.IsNullOrEmpty(modeStr))
            {
                return devMode;
            }

            //public enum ApInnerType
            //{
            //    GSM = 0,
            //    GSM_V2,
            //    TD_SCDMA,
            //    CDMA,
            //    WCDMA,
            //    LTE_TDD,
            //    LTE_FDD
            //}

            modeStr = modeStr.ToUpper();
            switch (modeStr)
            {
                case "GSM":
                    {
                        devMode = devMode.MODE_GSM;
                        break;
                    }
                case "GSM_V2":
                case "GSM-V2":
                    {
                        devMode = devMode.MODE_GSM_V2;
                        break;
                    }
                case "TD_SCDMA":
                case "TD-SCDMA":
                    {
                        devMode = devMode.MODE_TD_SCDMA;
                        break;
                    }
                case "CDMA":
                    {
                        devMode = devMode.MODE_CDMA;
                        break;
                    }
                case "WCDMA":
                    {
                        devMode = devMode.MODE_WCDMA;
                        break;
                    }
                case "LTE_TDD":
                case "LTE-TDD":
                    {
                        devMode = devMode.MODE_LTE_TDD;
                        break;
                    }
                case "LTE_FDD":
                case "LTE-FDD":
                    {
                        devMode = devMode.MODE_LTE_FDD;
                        break;
                    }
                default:
                    {
                        devMode = devMode.MODE_UNKNOWN;
                        break;
                    }
            }

            return devMode;
        }

        /// <summary>
        /// 获取mode类型获取对应的mode字符串
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string get_device_mode_string(devMode mode)
        {
            string modeStr = "";

            switch (mode)
            {
                case devMode.MODE_GSM:
                    {
                        modeStr = "GSM";
                        break;
                    }
                case devMode.MODE_GSM_V2:
                    {
                        modeStr = "GSM-V2";
                        break;
                    }
                case devMode.MODE_TD_SCDMA:
                    {
                        modeStr = "TD-SCDMA";
                        break;
                    }
                case devMode.MODE_CDMA:
                    {
                        modeStr = "CDMA";
                        break;
                    }
                case devMode.MODE_WCDMA:
                    {
                        modeStr = "WCDMA";
                        break;
                    }
                case devMode.MODE_LTE_TDD:
                    {
                        modeStr = "LTE-TDD";
                        break;
                    }
                case devMode.MODE_LTE_FDD:
                    {
                        modeStr = "LTE-FDD";
                        break;
                    }                                            
                case devMode.MODE_UNKNOWN:
                    {
                        modeStr = "UNKNOWN";
                        break;
                    }
                default:
                    {
                        modeStr = "UNKNOWN";
                        break;
                    }
            }

            return modeStr;
        }

        ///// <summary>
        ///// 用于快速通过设备的全名早点设备对应的ID
        ///// 如：设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3，其中
        ///// 设备.深圳.福田.中心广场.西北监控为域名，LTE-FDD-B3为名称
        ///// 系统启动后或设备有更改后获取该字典到内存中
        ///// string = 设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3
        ///// int    = device的id
        ///// </summary>
        ///// <param name="dic">返回的字典</param>
        ///// <returns>
        /////   RC.NO_OPEN   ：数据库尚未打开
        /////   RC.OP_FAIL   ：数据库操作失败 
        /////   RC.SUCCESS   ：成功
        ///// </returns>
        //public int domain_dictionary_info_join_get(ref Dictionary<string, int> dic)
        //{    
        //    if (false == myDbConnFlag)
        //    {
        //        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN]);
        //        return (int)RC.NO_OPEN;
        //    }

        //    dic = new Dictionary<string, int>();

        //    string sql = string.Format("SELECT a.nameFullPath,b.name,b.id FROM (select id,nameFullPath from domain where isStation = 1) AS a INNER JOIN device As b ON a.id = b.affDomainId");

        //    try
        //    {
        //        using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
        //        {
        //            using (MySqlDataReader dr = cmd.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    if (!string.IsNullOrEmpty(dr["nameFullPath"].ToString()) && 
        //                        !string.IsNullOrEmpty(dr["name"].ToString()) &&
        //                        !string.IsNullOrEmpty(dr["id"].ToString()))
        //                    {
        //                        string completeName = string.Format("{0}.{1}", dr["nameFullPath"].ToString(), dr["name"].ToString());

        //                        if (!dic.ContainsKey(completeName))
        //                        {
        //                            dic.Add(completeName, int.Parse(dr["id"].ToString()));
        //                        }
        //                    }                          
        //                }
        //                dr.Close();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
        //        return (int)RC.OP_FAIL;
        //    }

        //    return (int)RC.SUCCESS;
        //}

        /// <summary>
        /// 用于快速通过设备的全名早点设备对应的ID
        /// 如：设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3，其中
        /// 设备.深圳.福田.中心广场.西北监控为域名，LTE-FDD-B3为名称
        /// 系统启动后或设备有更改后获取该字典到内存中
        /// string = 设备.深圳.福田.中心广场.西北监控.LTE-FDD-B3
        /// strDevice = 设备的各个字段
        /// </summary>
        /// <param name="dic">返回的字典</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int domain_dictionary_info_join_get(ref Dictionary<string, strDevice> dic)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dic = new Dictionary<string, strDevice>();

            string sql = string.Format("SELECT a.nameFullPath,b.* FROM (select id,nameFullPath from domain where isStation = 1) AS a INNER JOIN device As b ON a.id = b.affDomainId");

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["nameFullPath"].ToString()) && !string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                string completeName = string.Format("{0}.{1}", dr["nameFullPath"].ToString(), dr["name"].ToString());

                                strDevice strDev = new strDevice();

                                //2018-07-20
                                strDev.wSelfStudy = "0";

                                if (!string.IsNullOrEmpty(dr["id"].ToString()))
                                {
                                    strDev.id = int.Parse(dr["id"].ToString());
                                }

                                if (!string.IsNullOrEmpty(dr["name"].ToString()))
                                {
                                    strDev.name = dr["name"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["sn"].ToString()))
                                {
                                    strDev.sn = dr["sn"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["ipAddr"].ToString()))
                                {
                                    strDev.ipAddr = dr["ipAddr"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["port"].ToString()))
                                {
                                    strDev.port = dr["port"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["netmask"].ToString()))
                                {
                                    strDev.netmask = dr["netmask"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["mode"].ToString()))
                                {
                                    strDev.mode = dr["mode"].ToString();
                                    strDev.devMode = get_device_mode(strDev.mode);
                                }

                                if (!string.IsNullOrEmpty(dr["innerType"].ToString()))
                                {
                                    strDev.innerType = dr["innerType"].ToString();
                                }

                                if (!string.IsNullOrEmpty(dr["affDomainId"].ToString()))
                                {
                                    strDev.affDomainId = dr["affDomainId"].ToString();
                                }

                                if (!dic.ContainsKey(completeName))
                                {
                                    dic.Add(completeName, strDev);
                                }
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取域表中的各条叶子节点记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,nameFullPath,isStation
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int domain_record_leaf_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("domain");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.Int32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "nameFullPath";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.Int32");
            column3.ColumnName = "isStation";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            DataTable dtAll = new DataTable();
            int rv = domain_record_entity_get(ref dtAll, 0);
            if (rv != (int)RC.SUCCESS)
            {
                return rv;
            }

            List<int> idList = new List<int>();
            List<int> parentIdList = new List<int>();
            List<int> leafIdList = new List<int>();

            foreach (DataRow dr in dtAll.Rows)
            {
                int id = int.Parse(dr["id"].ToString());
                int parentId = int.Parse(dr["parentId"].ToString());

                idList.Add(id);
                parentIdList.Add(parentId);
            }

            string subSql = "";
            foreach (int inx in idList)
            {
                if (!parentIdList.Contains(inx))
                {
                    leafIdList.Add(inx);
                    subSql += string.Format("id = {0} or ", inx);
                }
            }

            if (subSql != "")
            {
                subSql = subSql.Remove(subSql.Length - 3, 3);
            }

    
            string  sql = string.Format("select id,name,nameFullPath,isStation from domain where {0}",subSql);   

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToInt32(dr[0]);
                            row[1] = dr[1].ToString();
                            row[2] = dr[2].ToString();
                            row[3] = Convert.ToInt32(dr[3]);

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取一个节点下所有站点下所有设备Id的列表
        /// </summary>
        /// <param name="nameFullPath">节点的全路径名称</param>
        /// <param name="listDevId">所有设备Id的列表</param>
        /// <returns>
        ///   RC.NO_OPEN         ：数据库尚未打开
        ///   RC.PAR_NULL        ：参数为空
        ///   PAR_LEN_ERR        ：参数长度有误
        ///   RC.OP_FAIL         ：数据库操作失败 
        ///   RC.NO_EXIST        ：记录不存在        
        ///   RC.SUCCESS         ：成功
        /// </returns>
        public int domain_record_device_id_list_get(string nameFullPath, ref List<int> listDevId)
        {
            //int rtv = -1;
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (nameFullPath.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(nameFullPath))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            listDevId = new List<int>();

            #region 分成两步

            //List<int> listID = new List<int>();

            //rtv = domain_record_station_list_get(nameFullPath, ref listID);
            //if ((int)RC.SUCCESS != rtv)
            //{
            //    return rtv;
            //}

            //string sqlSub = "";
            //for (int i = 0; i < listID.Count; i++)
            //{
            //    if (i == (listID.Count - 1))
            //    {
            //        sqlSub += string.Format("affDomainId = {0} ", listID[i]);
            //    }
            //    else
            //    {
            //        sqlSub += string.Format("affDomainId = {0} or ", listID[i]);
            //    }
            //}


            //string sql = string.Format("select id from device where {0}",sqlSub);

            #endregion

            #region 一步搞定

            string sql = string.Format("SELECT b.id FROM (select id from domain where (nameFullPath like '{0}.%%' or nameFullPath = '{0}') and isStation = 1) AS a INNER JOIN device As b ON a.id = b.affDomainId", nameFullPath);

            #endregion

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                listDevId.Add(int.Parse(dr[0].ToString()));
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取域表中id和nameFullPath的字典
        /// </summary>
        /// <param name="dic">返回的字典</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int domain_dictionary_id_nameFullPath_get(ref Dictionary<string, string> dic)
        {          
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }
            
            string sql = string.Format("select id,nameFullPath from domain where isStation = 1");
           
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["id"].ToString()) && !string.IsNullOrEmpty(dr["nameFullPath"].ToString()))
                            {
                                dic.Add(dr["id"].ToString(), dr["nameFullPath"].ToString());
                            }                       
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 08-userdomain操作

        /// <summary>
        /// 检查用户-域的记录是否存在
        /// </summary>
        /// <param name="usrName"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int userdomain_record_exist(string usrName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 )
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from userdomain where usrName = '{0}'", usrName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查用户-域的记录是否存在
        /// </summary>
        /// <param name="usrName"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int userdomain_record_exist(int usrDomainId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }          

            string sql = string.Format("select count(*) from userdomain where usrDomainId = {0}", usrDomainId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 在权限表的ID集合中，检查ID集合的合法性
        /// </summary>
        /// <param name="listStr">原始权限ID集合</param>
        /// <param name="listIdSetOk">校验后合法的ID集合</param>
        /// <returns>
        /// true  ： 成功
        /// false ： 失败
        /// </returns>
        private bool check_and_get_domainid_set_in_db(List<string> listStr, ref List<string> listIdSetOk)
        {
            if (listStr.Count == 0)
            {
                return false;
            }

            listIdSetOk = new List<string>();

            List<string> listStationIdSet = new List<string>();
            if ((int)RC.SUCCESS == domain_record_station_id_set_get(ref listStationIdSet))
            {
                if (listStationIdSet.Count > 0)
                {
                    foreach (string str in listStr)
                    {
                        if (listStationIdSet.Contains(str))
                        {
                            listIdSetOk.Add(str);
                        }
                    }
                }
            }

            if (listIdSetOk.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 插入记录到用户权限表中,在此，一个用户只能指定到一个角色中
        /// </summary>
        /// <param name="usrName">用户名</param>
        /// <param name="domainIdSet">
        /// 域ID集合，即该用户可以访问的站点集合，如下：1,2,3,4,5，每个域用逗号隔开
        /// </param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   EXIST          ：记录已经存在
        ///   USR_NO_EXIST   ：userName不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int userdomain_record_insert(string usrName, string domainIdSet, string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName) ||
                string.IsNullOrEmpty(domainIdSet) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 || des.Length > 256 || domainIdSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查权限ID集合的合法性
            if (false == check_id_set(domainIdSet))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_ERR;
            }
            else
            {
                List<string> listDomainIdSetOri = new List<string>();
                List<string> listDomainIdSetChk = new List<string>();

                if (false == check_and_get_id_set(domainIdSet, ref listDomainIdSetOri))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }

                if (false == check_and_get_domainid_set_in_db(listDomainIdSetOri, ref listDomainIdSetChk))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }
                else
                {
                    domainIdSet = "";
                    for (int i = 0; i < listDomainIdSetChk.Count; i++)
                    {
                        if (i == (listDomainIdSetChk.Count - 1))
                        {
                            domainIdSet += listDomainIdSetChk[i];
                        }
                        else
                        {
                            domainIdSet += listDomainIdSetChk[i] + ",";
                        }
                    }
                }
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }

            //检查用户域记录是否存在
            if ((int)RC.EXIST == userdomain_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }         

            string sql = string.Format("insert into userdomain values(NULL,'{0}','{1}','{2}')", usrName, domainIdSet, des);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在用户-域表中删除指定的记录 
        /// </summary>
        /// <param name="usrName"></param>
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int userdomain_record_delete(string usrName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.USR_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }            

            //检查记录是否存在
            if ((int)RC.NO_EXIST == userdomain_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from userdomain where usrName = '{0}'", usrName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到用户-域表中
        /// </summary>
        /// <param name="usrName"></param>
        /// <param name="domainIdSet">站点ID集合，如下：6,9,每个权限用逗号隔开</param>
        /// <param name="des"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   USR_NO_EXIST   ：用户不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int userdomain_record_update(string usrName, string domainIdSet, string des)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName) ||
                string.IsNullOrEmpty(domainIdSet) ||
                string.IsNullOrEmpty(des))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64 || domainIdSet.Length > 1024 || des.Length > 256)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查权限ID集合的合法性
            if (false == check_id_set(domainIdSet))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_ERR;
            }
            else
            {
                List<string> listDomainIdSetOri = new List<string>();
                List<string> listDomainIdSetChk = new List<string>();

                if (false == check_and_get_id_set(domainIdSet, ref listDomainIdSetOri))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }

                if (false == check_and_get_domainid_set_in_db(listDomainIdSetOri, ref listDomainIdSetChk))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_ERR], "DB", LogCategory.I);
                    return (int)RC.ID_SET_ERR;
                }
                else
                {
                    domainIdSet = "";
                    for (int i = 0; i < listDomainIdSetChk.Count; i++)
                    {
                        if (i == (listDomainIdSetChk.Count - 1))
                        {
                            domainIdSet += listDomainIdSetChk[i];
                        }
                        else
                        {
                            domainIdSet += listDomainIdSetChk[i] + ",";
                        }
                    }
                }
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }

            //检查用户域记录是否存在
            if ((int)RC.NO_EXIST == userdomain_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("update userdomain set domainIdSet='{0}',des='{1}' where usrName='{2}'", domainIdSet, des, usrName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取用户-域表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：usrDomainId,usrName,domainIdSet,des
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int userdomain_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("userdomain");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "usrDomainId";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "usrName";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "domainIdSet";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);

            string sql = string.Format("select * from userdomain");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToUInt32(dr[0]);
                            row[1] = dr[1].ToString();
                            row[2] = dr[2].ToString();

                            if (!string.IsNullOrEmpty(dr["des"].ToString()))
                            {
                                row["des"] = dr["des"].ToString();
                            }
                            else
                            {
                                row["des"] = "";
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取用户-域表中的各条记录
        /// </summary>
        /// <param name="dic">
        /// int = usrDomainId
        /// List<int> = domainIdSet
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int userdomain_record_entity_get(ref Dictionary<int,List<string>> dic)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dic = new Dictionary<int, List<string>>();

            string sql = string.Format("select usrDomainId,domainIdSet from userdomain");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["domainIdSet"].ToString()))
                            {
                                int usrDomainId = Convert.ToInt32(dr["usrDomainId"]);
                                List<string> idSet = new List<string>();

                                if (true == check_and_get_id_set(dr["domainIdSet"].ToString(), ref idSet))
                                {
                                    dic.Add(usrDomainId, idSet);
                                }
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到用户-域表中
        /// </summary>
        /// <param name="usrDomainId"></param>
        /// <param name="domainIdSet">站点ID集合，如下：6,9,每个权限用逗号隔开</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   USR_NO_EXIST   ：用户不存在
        ///   ID_SET_ERR     ：ID集合有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int userdomain_record_update(int usrDomainId, string domainIdSet)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (domainIdSet == null)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (domainIdSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }


            //检查用户域记录是否存在
            if ((int)RC.NO_EXIST == userdomain_record_exist(usrDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = "";
            if (domainIdSet == "")
            {
                sql = string.Format("delete from userdomain where usrDomainId = {0}", usrDomainId);
            }
            else
            {
                sql = string.Format("update userdomain set domainIdSet='{0}' where usrDomainId = {1}", domainIdSet, usrDomainId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过用户名获取用户所有的域权限
        /// </summary>
        /// <param name="usrName">用户名</param>
        /// <param name="listDomain">返回的域集合列表</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.USR_NO_EXIST   ：用户不存在
        ///   RC.ID_SET_FMT_ERR ：ID集合格式有误
        ///   RC.SUCCESS        ：成功
        /// </returns>
        public int userdomain_set_get_by_usrname(string usrName, ref List<string> listDomain)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (usrName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查用户是否存在
            if ((int)RC.NO_EXIST == user_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.USR_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.USR_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == userdomain_record_exist(usrName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string domainIdSet = "";
            string sql = string.Format("select domainIdSet from userdomain where usrName = '{0}'", usrName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {                            
                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                domainIdSet = dr[0].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }


            List<string> listStr = new List<string>();
            if (!check_and_get_id_set(domainIdSet, ref listStr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.ID_SET_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.ID_SET_FMT_ERR;                
            }

            string nameFullPath = "";
            listDomain = new List<string>();
            foreach (string str in listStr)
            {
                if ((int)RC.SUCCESS == domain_get_nameFullPath_by_id(str, ref nameFullPath))
                {
                    listDomain.Add(nameFullPath);
                }
                else
                {
                    return (int)RC.OP_FAIL ;
                }
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 09-device操作

        /// <summary>
        /// 检查设备记录是否存在
        /// 用域名+设备名来区分，如：设备.深圳.福田.中心广场.西北监控.LTE-FDD
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int device_record_exist(int affDomainId,string name)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from device where affDomainId = {0} and name = '{1}'", affDomainId,name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查设备记录是否存在
        /// </summary>
        /// <param name="devId">设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int device_record_exist(int devId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }
            
            string sql = string.Format("select count(*) from device where id = {0}", devId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到设备表中
        /// 同时，生成和该设备相关的其他表
        /// </summary>
        /// <param name="affDomainId">所属域ID</param>
        /// <param name="name"></param>
        /// <param name="mode">
        /// 制式：GSM,TD-SCDMA,WCDMA,LTE-TDD,LTE-FDD 
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.EXIST          ：记录已经存在
        ///   RC.IS_NOT_STATION ：域ID不是站点
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int device_record_insert(int affDomainId, string name, string mode)
        {
            int rtv1 = -1;
            int rtv2 = -1;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            devMode dm = get_device_mode(mode);
            if (dm == devMode.MODE_UNKNOWN)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.AP_MODE_ERR], "DB", LogCategory.I);
                return (int)RC.AP_MODE_ERR;
            }

            //检查域ID是否为站点
            if ((int)RC.IS_NOT_STATION == domain_record_is_station(affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IS_NOT_STATION], "DB", LogCategory.I);
                return (int)RC.IS_NOT_STATION;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == device_record_exist(affDomainId, name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            #region comment

            //Int32 maxId = 0;
            //string sql = string.Format("select max(id) from device");
            //try
            //{
            //    using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
            //    {
            //        using (MySqlDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                maxId = Convert.ToInt32(dr[0]);
            //            }
            //            dr.Close();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
            //    return (int)RC.OP_FAIL;
            //}

            //if (mode != "GSM")
            //{
            //    string sql = string.Format("insert into device(name, mode,affDomainId) values('{0}','{1}',{2})", name, mode, affDomainId);

            //    try
            //    {
            //        using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
            //        {
            //            if (cmd.ExecuteNonQuery() < 0)
            //            {
            //                Logger.Trace(LogInfoType.WARN, sql);
            //                return (int)RC.OP_FAIL;
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
            //        return (int)RC.OP_FAIL;
            //    }
            //}
            //else
            //{
            //    for (int carry = 0; carry <= 1; carry++)
            //    {
            //        string sql = string.Format("insert into device values(NULL,'{0}',NULL,{1},-1,NULL,NULL,NULL,'{2}',NULL,NULL,1,NULL,{3})", name, carry, mode, affDomainId);
            //        try
            //        {
            //            using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
            //            {
            //                if (cmd.ExecuteNonQuery() < 0)
            //                {
            //                    Logger.Trace(LogInfoType.WARN, sql);
            //                    return (int)RC.OP_FAIL;
            //                }
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
            //            return (int)RC.OP_FAIL;
            //        }
            //    }
            //}

            #endregion

            string sql = string.Format("insert into device(name, mode,affDomainId) values('{0}','{1}',{2})", name, mode, affDomainId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            int id = -1;
            string modeFromDev = "";            

            /*
             * 通过所属域ID和名称获取对应的设备id列表和mode
             */
            rtv1 = device_record_id_get_by_affdomainid_and_name(affDomainId, name, ref id, ref modeFromDev);
            if (rtv1 != 0)
            {
                return rtv1;
            }

            switch (dm)
            {
                case devMode.MODE_GSM:
                    {
                        #region GSM

                        rtv2 = gsm_sys_para_record_insert(0, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gsm_sys_para_record_insert(1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gsm_rf_para_record_insert(0, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gsm_rf_para_record_insert(1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gsm_sys_option_record_insert(0, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gsm_sys_option_record_insert(1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        break;

                        #endregion
                    }
                case devMode.MODE_GSM_V2:
                    {
                        #region GSM-V2

                        // 2108-07-29

                        rtv2 = ap_status_record_insert(id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gc_param_config_record_insert(0, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gc_param_config_record_insert(1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gc_misc_record_insert(0, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gc_misc_record_insert(1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        break;

                        #endregion
                    }
                case devMode.MODE_CDMA:
                    {
                        #region CDMA

                        // 2108-07-29

                        rtv2 = gc_param_config_record_insert(-1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = gc_misc_record_insert(-1, id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        break;

                        #endregion
                    }
                case devMode.MODE_TD_SCDMA:
                    {
                        break;
                    }
                case devMode.MODE_WCDMA:
                case devMode.MODE_LTE_FDD:
                case devMode.MODE_LTE_TDD:
                    {
                        #region LTE

                        rtv2 = ap_status_record_insert(id);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        rtv2 = ap_general_para_record_insert(id, mode);
                        if (rtv2 != 0)
                        {
                            return rtv2;
                        }

                        break;

                        #endregion
                    }
                case devMode.MODE_UNKNOWN:
                    {
                        break;
                    }
            }

            return 0;
        }

        /// <summary>
        /// 是否可以进行重命名设备的名称
        /// </summary>
        /// <param name="affDomainId">所属域ID</param>
        /// <param name="oldName">旧名称</param>
        /// <param name="newName">新名称</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败
        ///   RC.IS_NOT_STATION ：域ID不是站点
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.MODIFIED_EXIST ：修改后的记录已经存在
        ///   RC.SUCCESS        ：成功(可以重命名) 
        /// </returns>
        public int device_record_if_rename(int affDomainId, string oldName,string newName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (oldName.Length > 64 || newName.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查域ID是否为站点
            if ((int)RC.IS_NOT_STATION == domain_record_is_station(affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IS_NOT_STATION], "DB", LogCategory.I);
                return (int)RC.IS_NOT_STATION;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDomainId, oldName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("select count(*) from device where affDomainId = {0} and name != '{1}' and name = '{2}'", affDomainId,oldName, newName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.MODIFIED_EXIST;
            }
            else
            {
                return (int)RC.SUCCESS;
            }
        }

        /// <summary>
        /// 通过域ID和名称更新记录到设备表中
        /// </summary>
        /// <param name="affDomainId"></param>
        /// <param name="name"></param>
        /// <param name="dev">
        /// 更新的字段如下，那些字段不为空就更新那些
        /// 
        /// sn;            //SN，GSM或第三方设备可能没有该字段
        /// carry;       //仅用于标识GSM的载波，0：载波0；1：载波1
        /// bindingDevId;  //仅用于标识GSM的绑定设备id
        /// ipAddr;        //IP地址
        /// port;          //端口号
        /// netmask;       //掩码
        /// mode;          //设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等
        /// online;        //上下线标识，0：下线；1：上线
        /// lastOnline;    //最后的上线时间
        /// isActive;      //标识该设备是否生效，0：无效；1：生效
        /// innerType;     //用于软件内部处理
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.MODIFIED_EXIST ：修改后的记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int device_record_update(int affDomainId, string name,strDevice dev)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST== device_record_exist(affDomainId, name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //检查修改后的记录是否存在
            if (!string.IsNullOrEmpty(dev.name))
            {
                if ((int)RC.MODIFIED_EXIST == device_record_if_rename(affDomainId, name, dev.name))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.MODIFIED_EXIST], "DB", LogCategory.I);
                    return (int)RC.MODIFIED_EXIST;
                }              
            }


            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(dev.name))
            {
                if (dev.name.Length > 64)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("name = '{0}',", dev.name);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(dev.sn))
            {
                if (dev.sn.Length > 32)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("sn = '{0}',", dev.sn);
                }
            }


            //(5)
            if (!string.IsNullOrEmpty(dev.ipAddr))
            {
                IPAddress ip;
                if (IPAddress.TryParse(dev.ipAddr, out ip))
                {
                    sqlSub += string.Format("ipAddr = '{0}',", dev.ipAddr);
                }
                else
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }                
            }

            //(6)
            if (!string.IsNullOrEmpty(dev.port))
            {
                try
                {
                    int port = int.Parse(dev.port);

                    if (port > 65535)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.PAR_FMT_ERR;
                    }
                    else
                    {
                        sqlSub += string.Format("port = {0},", dev.port);
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }                
            }

            //(7)
            if (!string.IsNullOrEmpty(dev.netmask))
            {
                if (dev.netmask.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("netmask = '{0}',", dev.netmask);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(dev.mode))
            {
                if (dev.mode.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("mode = '{0}',", dev.mode);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(dev.online))
            {
                if ((dev.online == "0") || (dev.online == "1"))
                {
                    sqlSub += string.Format("online = {0},", dev.online);
                }
            }


            //(10)
            if (!string.IsNullOrEmpty(dev.lastOnline))
            {
                try
                {
                    DateTime.Parse(dev.lastOnline);                    
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                    return (int)RC.TIME_FMT_ERR;
                }

                sqlSub += string.Format("lastOnline = '{0}',", dev.lastOnline);
            }

            //(11)
            if (!string.IsNullOrEmpty(dev.isActive))
            {
                if ((dev.isActive == "0") && (dev.isActive == "1"))
                {
                    sqlSub += string.Format("isActive = {0},", dev.isActive);
                }                    
            }

            //(12)
            if (!string.IsNullOrEmpty(dev.innerType))
            {
                if (dev.innerType.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("innerType = '{0}',", dev.innerType);
                }
            }


            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update device set {0} where name = '{1}' and affDomainId = {2}", sqlSub, name, affDomainId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 将所有设备设置为下线
        /// </summary>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int device_record_clear_online()
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            
            string sql = string.Format("update device set online = 0");

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在设备表中删除指定的记录 
        /// </summary>  
        /// <param name="affDomainId">域ID</param>
        /// <param name="name">设备名称</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int device_record_delete(int affDomainId, string name)
        {
            int rtv = -1;
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDomainId, name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string modeFromDev = "";
            int id = -1;

            rtv = device_record_id_get_by_affdomainid_and_name(affDomainId, name, ref id, ref modeFromDev);

            string sql = string.Format("delete from device where affDomainId = {0} and name = '{1}'", affDomainId, name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (rtv != 0)
            {
                return rtv;
            }

            devMode dm = get_device_mode(modeFromDev);
            if (dm == devMode.MODE_UNKNOWN)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.AP_MODE_ERR], "DB", LogCategory.I);
                return (int)RC.AP_MODE_ERR;
            }

            switch (dm)
            {
                case devMode.MODE_GSM:
                    {
                        #region GSM

                        rtv = 0;
                        rtv += gsm_sys_para_record_delete(0, id);
                        rtv += gsm_sys_para_record_delete(1, id);

                        rtv += gsm_rf_para_record_delete(0, id);
                        rtv += gsm_rf_para_record_delete(1, id);

                        rtv += gsm_sys_option_record_delete(0, id);
                        rtv += gsm_sys_option_record_delete(1, id);

                        rtv += bwlist_record_delete(id);

                        break;

                        #endregion
                    }
                case devMode.MODE_GSM_V2:
                    {
                        #region GSM-V2

                        // 2108-07-29

                        rtv = 0;
                        rtv += gc_param_config_record_delete(0, id);
                        rtv += gc_param_config_record_delete(1, id);

                        rtv += gc_misc_record_delete(0, id);
                        rtv += gc_misc_record_delete(1, id);

                        rtv += gc_nb_cell_record_delete(0, id);
                        rtv += gc_nb_cell_record_delete(1, id);

                        rtv += gc_imsi_action_record_delete(0, id);
                        rtv += gc_imsi_action_record_delete(1, id);

                        rtv += bwlist_record_delete(id);

                        break;

                        #endregion
                    }
                case devMode.MODE_TD_SCDMA:
                    {
                        break;
                    }
                case devMode.MODE_CDMA:
                    {
                        #region CDMA

                        // 2108-07-29

                        rtv = 0;
                        rtv += gc_param_config_record_delete(-1, id);
                        rtv += gc_misc_record_delete(-1, id);
                        rtv += gc_nb_cell_record_delete(-1, id);
                        rtv += gc_imsi_action_record_delete(-1, id);
                        rtv += bwlist_record_delete(id);

                        break;

                        #endregion
                    }
                case devMode.MODE_WCDMA:
                case devMode.MODE_LTE_FDD:
                case devMode.MODE_LTE_TDD:
                    {
                        #region LTE

                        rtv = 0;

                        //在AP状态表中删除指定的记录 
                        rtv += ap_status_record_delete(id);
                        rtv += ap_general_para_record_delete(id);
                        rtv += bwlist_record_delete(id);

                        break;

                        #endregion
                    }
                case devMode.MODE_UNKNOWN:
                    {
                        break;
                    }
            }

            return rtv;
        }

        /// <summary>
        /// 通过所属域ID和名称获取对应的设备id和mode
        /// </summary>  
        /// <param name="affDomainId">域ID</param>
        /// <param name="name">设备名称</param>
        /// <param name="listid">成功时返回的设备id列表</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int device_record_id_get_by_affdomainid_and_name(int affDomainId, string name,ref int id,ref string mode)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDomainId, name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            mode = "";
            id = -1;        

            string sql = string.Format("select id,mode from device where affDomainId = {0} and name = '{1}'", affDomainId, name);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            id = Convert.ToInt32(dr[0]);

                            if (!string.IsNullOrEmpty(dr[1].ToString()))
                            {
                                mode = dr[1].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过ipaddr和name获取对应的设备id列表（用于GSM）
        /// </summary>  
        /// <param name="ipAddr">ipaddr</param>
        /// <param name="name">设备名称</param>
        /// <param name="listid">成功时返回的设备id列表</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int device_record_id_get_by_ipaddr_and_name(string ipAddr, string name,ref List<int> listid)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr) || string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32 || name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }
           
            listid = new List<int>();

            //string sql = string.Format("select id from device where  ipAddr = '{0}' and name = '{1}' and  innerType = 'GSM'", ipAddr, name);

            string sql = string.Format("select id from device where  ipAddr = '{0}' and name = '{1}'", ipAddr, name);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            listid.Add(Convert.ToInt32(dr[0]));                           
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取设备表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,sn,ipAddr
        /// port,netmask,mode,online,lastOnline,isActive,innerType,affDomainId
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("device");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "sn";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "ipAddr";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.UInt16");
            column4.ColumnName = "port";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "netmask";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "mode";

            DataColumn column7 = new DataColumn();
            column7.DataType = System.Type.GetType("System.Int16");
            column7.ColumnName = "online";

            DataColumn column8 = new DataColumn();
            column8.DataType = System.Type.GetType("System.String");
            column8.ColumnName = "lastOnline";

            DataColumn column9 = new DataColumn();
            column9.DataType = System.Type.GetType("System.Int16");
            column9.ColumnName = "isActive";

            DataColumn column10 = new DataColumn();
            column10.DataType = System.Type.GetType("System.String");
            column10.ColumnName = "innerType";

            DataColumn column11 = new DataColumn();
            column11.DataType = System.Type.GetType("System.Int32");
            column11.ColumnName = "affDomainId";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);

            string sql = string.Format("select * from device");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToUInt32(dr["id"]);
                            row["name"] = dr["name"].ToString();
                            row["sn"] = dr["sn"].ToString();                            

                            row["ipAddr"] = dr["ipAddr"].ToString();

                            if (dr["port"].ToString() != "")
                            {
                                row["port"] = Convert.ToUInt16(dr["port"]);
                            }
                            else
                            {
                                row["port"] = 0;
                            }

                            row["netmask"] = dr["netmask"].ToString();
                            row["mode"] = dr["mode"].ToString();

                            if (dr["online"].ToString() != "")
                            {
                                row["online"] = Convert.ToInt16(dr["online"]);
                            }
                            else
                            {
                                row["online"] = 0;
                            }

                            row["lastOnline"] = dr["lastOnline"].ToString();

                            if (dr["isActive"].ToString() != "")
                            {
                                row["isActive"] = Convert.ToInt16(dr["isActive"]);
                            }
                            else
                            {
                                row["isActive"] = 0;
                            }

                            row["innerType"] = dr["innerType"].ToString();

                            if (dr["affDomainId"].ToString() != "")
                            {
                                row["affDomainId"] = Convert.ToInt32(dr["affDomainId"]);
                            }
                            else
                            {
                                row["affDomainId"] = -1;
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID获取对应的记录
        /// </summary>
        /// <param name="dev">返回记录对应的结构体</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.NO_EXIST  ：记录不存在 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_record_entity_get_by_devid(int devId, ref strDevice dev)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if ((int)RC.NO_EXIST == device_record_exist(devId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            dev = new strDevice();

            string sql = string.Format("select * from device where id = {0}", devId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["name"].ToString()))
                            {
                                dev.name = dr["name"].ToString();
                            }
                            else
                            {
                                dev.name = "";
                            }

                            if (!string.IsNullOrEmpty(dr["sn"].ToString()))
                            {
                                dev.sn = dr["sn"].ToString();
                            }
                            else
                            {
                                dev.sn = "";
                            }

                            if (!string.IsNullOrEmpty(dr["ipAddr"].ToString()))
                            {
                                dev.ipAddr = dr["ipAddr"].ToString();
                            }
                            else
                            {
                                dev.ipAddr = "";
                            }

                            if (!string.IsNullOrEmpty(dr["port"].ToString()))
                            {
                                dev.port = dr["port"].ToString();
                            }
                            else
                            {
                                dev.port = "";
                            }

                            if (!string.IsNullOrEmpty(dr["mode"].ToString()))
                            {
                                dev.mode = dr["mode"].ToString();
                            }
                            else
                            {
                                dev.mode = "";
                            }

                            if (!string.IsNullOrEmpty(dr["online"].ToString()))
                            {
                                dev.online = dr["online"].ToString();
                            }
                            else
                            {
                                dev.online = "";
                            }

                            if (!string.IsNullOrEmpty(dr["innerType"].ToString()))
                            {
                                dev.innerType = dr["innerType"].ToString();
                            }
                            else
                            {
                                dev.innerType = "";
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过所属域ID获取设备表中的各条记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="affDomainId"></param>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,sn,ipAddr
        /// port,netmask,mode,online,lastOnline,isActive,innerType,affDomainId
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.PAR_NULL  ：参数为空
        ///   PAR_LEN_ERR  ：参数长度有误
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_record_entity_get_by_name_affdomainid(string name,int affDomainId, ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(name))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (name.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            dt = new DataTable("device");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "sn";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "ipAddr";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.UInt16");
            column4.ColumnName = "port";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "netmask";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "mode";

            DataColumn column7 = new DataColumn();
            column7.DataType = System.Type.GetType("System.Int16");
            column7.ColumnName = "online";

            DataColumn column8 = new DataColumn();
            column8.DataType = System.Type.GetType("System.String");
            column8.ColumnName = "lastOnline";

            DataColumn column9 = new DataColumn();
            column9.DataType = System.Type.GetType("System.Int16");
            column9.ColumnName = "isActive";

            DataColumn column10 = new DataColumn();
            column10.DataType = System.Type.GetType("System.String");
            column10.ColumnName = "innerType";

            DataColumn column11 = new DataColumn();
            column11.DataType = System.Type.GetType("System.Int32");
            column11.ColumnName = "affDomainId";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);

            string sql = string.Format("select * from device where name = '{0}' and affDomainId = {1}", name,affDomainId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToUInt32(dr["id"]);
                            row["name"] = dr["name"].ToString();
                            row["sn"] = dr["sn"].ToString();                            

                            row["ipAddr"] = dr["ipAddr"].ToString();

                            if (dr["port"].ToString() != "")
                            {
                                row["port"] = Convert.ToUInt16(dr["port"].ToString());
                            }
                            else
                            {
                                row["port"] = 0;
                            }

                            row["netmask"] = dr["netmask"].ToString();
                            row["mode"] = dr["mode"].ToString();

                            if (dr["online"].ToString() != "")
                            {
                                row["online"] = Convert.ToInt16(dr["online"].ToString());
                            }
                            else
                            {
                                row["online"] = 0;
                            }

                            row["lastOnline"] = dr["lastOnline"].ToString();

                            if (dr["isActive"].ToString() != "")
                            {
                                row["isActive"] = Convert.ToInt16(dr["isActive"].ToString());
                            }
                            else
                            {
                                row["isActive"] = 0;
                            }

                            row["innerType"] = dr["innerType"].ToString();

                            if (dr["affDomainId"].ToString() != "")
                            {
                                row["affDomainId"] = Convert.ToInt32(dr["affDomainId"].ToString());
                            }
                            else
                            {
                                row["affDomainId"] = -1;
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过所属的域ID获取设备表中的各条记录的id和name列表
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_id_name_get_by_affdomainid(int affDomainId,ref List<string> listId,ref List<string> listName)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            listId = new List<string>();
            listName = new List<string>();

            string sql = string.Format("select id,name from device where affDomainId = {0} group by name", affDomainId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            if (!string.IsNullOrEmpty(dr[0].ToString()))
                            {
                                listId.Add(dr[0].ToString());
                            }

                            if (!string.IsNullOrEmpty(dr[1].ToString()))
                            {
                                listName.Add(dr[1].ToString());
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取所有上线或下线设备的个数
        /// </summary>
        /// <param name="onOffLineFlag">
        /// 上下线标识
        /// 1 ： 上线
        /// 0 ： 下线
        /// </param>
        /// <param name="count">成功时返回的个数</param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   RC.PAR_FMT_ERR ：参数格式有误
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int device_get_all_onoffline_count(int onOffLineFlag,ref UInt32 count)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (onOffLineFlag != 0 && onOffLineFlag!= 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }

            string sql = string.Format("select count(*) from device where online = {0}", onOffLineFlag);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            count = cnt;
            return (int)RC.SUCCESS;
        }

        #endregion

        #region 10-capture操作

        /// <summary>
        /// 插入记录到设备表中
        /// </summary>
        /// <param name="affDomainId">所属域ID</param>
        /// <param name="name"></param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int capture_record_insert(strCapture cap)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(cap.time) || string.IsNullOrEmpty(cap.affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            try
            {
                DateTime.Parse(cap.time);
                int.Parse(cap.affDeviceId);
            }
            catch (Exception ee)
            {
                Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }


            string sqlSub = "NULL,";

            //(1)
            if (!string.IsNullOrEmpty(cap.imsi))
            {
                if (cap.imsi.Length > 15)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("'{0}',", cap.imsi);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(2)
            if (!string.IsNullOrEmpty(cap.imei))
            {
                if (cap.imei.Length > 15)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("'{0}',", cap.imei);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(3)
            switch (cap.bwFlag)
            {
                case bwType.BWTYPE_WHITE:
                    {                  
                        sqlSub += string.Format("{0},", (int)cap.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_BLACK:
                    {
                        sqlSub += string.Format("{0},", (int)cap.bwFlag);
                        break;
                    }                
                case bwType.BWTYPE_OTHER:
                    {
                        sqlSub += string.Format("{0},", (int)cap.bwFlag);
                        break;
                    }                
                default:
                    {
                        //插入非数据库中定义的类型时，该字段显示为空值
                        sqlSub += string.Format("{0},", cap.bwFlag);
                        break;
                    }
            }

            //(4)
            if (!string.IsNullOrEmpty(cap.isdn))
            {
                if (cap.isdn.Length > 10)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", int.Parse(cap.isdn));
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(5)
            if (!string.IsNullOrEmpty(cap.bsPwr))
            {
                if (cap.bsPwr.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", cap.bsPwr);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(6)
            if (!string.IsNullOrEmpty(cap.tmsi))
            {
                if (cap.tmsi.Length > 10)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", cap.tmsi);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(7)
            if (!string.IsNullOrEmpty(cap.time))
            {
                if (cap.time.Length > 19)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("'{0}',", cap.time);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //(8)
            if (!string.IsNullOrEmpty(cap.affDeviceId))
            {
                if (cap.affDeviceId.Length > 10)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", cap.affDeviceId);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }

            string sql = string.Format("insert into capture values({0})", sqlSub);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 插入记录到设备表中
        /// </summary>
        /// <param name="affDomainId">所属域ID</param>
        /// <param name="name"></param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int capture_record_insert_batch(List<strCapture> listSC)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (listSC.Count <= 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            string sqlSub = "";
            string sqlBig = "";

            foreach (strCapture cap in listSC)
            {
                sqlSub = "NULL,";

                if (string.IsNullOrEmpty(cap.time) || string.IsNullOrEmpty(cap.affDeviceId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                    continue;
                }

                try
                {
                    DateTime.Parse(cap.time);
                    int.Parse(cap.affDeviceId);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                    continue;
                }                

                //(1)
                if (!string.IsNullOrEmpty(cap.imsi))
                {
                    if (cap.imsi.Length > 15)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", cap.imsi);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(2)
                if (!string.IsNullOrEmpty(cap.imei))
                {
                    if (cap.imei.Length > 15)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", cap.imei);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(3)
                switch (cap.bwFlag)
                {
                    case bwType.BWTYPE_WHITE:
                        {
                            sqlSub += string.Format("{0},", (int)cap.bwFlag);
                            break;
                        }
                    case bwType.BWTYPE_BLACK:
                        {
                            sqlSub += string.Format("{0},", (int)cap.bwFlag);
                            break;
                        }
                    case bwType.BWTYPE_OTHER:
                        {
                            sqlSub += string.Format("{0},", (int)cap.bwFlag);
                            break;
                        }
                    default:
                        {
                            //插入非数据库中定义的类型时，该字段显示为空值
                            sqlSub += string.Format("{0},", (int)cap.bwFlag);
                            break;
                        }
                }

                //(4)
                if (!string.IsNullOrEmpty(cap.isdn))
                {
                    if (cap.isdn.Length > 10)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", int.Parse(cap.isdn));
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(5)
                if (!string.IsNullOrEmpty(cap.bsPwr))
                {
                    if (cap.bsPwr.Length > 4)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", cap.bsPwr);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(6)
                if (!string.IsNullOrEmpty(cap.tmsi))
                {
                    if (cap.tmsi.Length > 10)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", cap.tmsi);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(7)
                if (!string.IsNullOrEmpty(cap.time))
                {
                    if (cap.time.Length > 19)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", cap.time);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(8)
                if (!string.IsNullOrEmpty(cap.affDeviceId))
                {
                    if (cap.affDeviceId.Length > 10)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", cap.affDeviceId);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                if (sqlSub != "")
                {
                    //去掉最后一个字符
                    sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
                    sqlSub = string.Format("({0}),", sqlSub);

                    sqlBig += sqlSub;
                }
            }

            if (sqlBig != "")
            {
                //去掉最后一个字符
                sqlBig = sqlBig.Remove(sqlBig.Length - 1, 1);
            }
            else
            {
                Logger.Trace(LogInfoType.EROR, "sqlBig is NULL:" + dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }


            string sql = string.Format("insert into capture values{0}", sqlBig);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取满足条件的捕号记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：imsi,imei,name,tmsi,time,bwFlag,sn
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN            ：数据库尚未打开
        ///   RC.OP_FAIL            ：数据库操作失败 
        ///   RC.CAP_QUERY_INFO_ERR ：捕号查询信息有误
        ///   RC.DEV_NO_EXIST       ：设备不存在
        ///   RC.TIME_ST_EN_ERR     ：开始时间大于结束时间
        ///   RC.SUCCESS            ：成功 
        /// </returns>
        public int capture_record_entity_query(ref DataTable dt,strCaptureQuery cq)
        {
            /// <summary>
            /// 当affDeviceId为-1时，表示搜索所有的设备
            /// </summary>
            //public int affDeviceId;     //所属设备ID       

            //public string imsi;         //IMSI号
            //public string imei;         //IMEI号       
            //public bwType bwFlag;       //名单标识
            //public string timeStart;    //开始时间
            //public string timeEnded;    //结束时间
            //public int RmDupFlag;       //是否对设备名称和SN去重标志，0:不去重，1:去重
            //public UInt32 topCount;     //返回最前面的记录数，0:所有，非0:指定的记录数         

            string RmDupFlagString = "";
            string topCountString = "";

            string sqlSub  = "";

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (cq.affDeviceId >= 1)
            {
                //检查设备是否存在
                if ((int)RC.NO_EXIST == device_record_exist(cq.affDeviceId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                    return (int)RC.DEV_NO_EXIST;
                }
                else
                {
                    sqlSub += string.Format("affDeviceId={0} and ", cq.affDeviceId);
                }
            }
            else if (cq.affDeviceId == -1)
            {
                sqlSub += "";
            }
            else
            {
                if (cq.listAffDeviceId.Count > 0)
                {
                    for (int i = 0; i < cq.listAffDeviceId.Count; i++)
                    {
                        if (i == (cq.listAffDeviceId.Count - 1))
                        {
                            sqlSub += string.Format("affDeviceId={0}", cq.listAffDeviceId[i]);
                        }
                        else
                        {
                            sqlSub += string.Format("affDeviceId={0} or ", cq.listAffDeviceId[i]);
                        }
                    }

                    sqlSub = string.Format("({0}) and ", sqlSub);
                }
                else
                {
                    dt = new DataTable("capturequery");
                    return (int)RC.SUCCESS;
                }
            }

            if (!string.IsNullOrEmpty(cq.imsi))
            {
                sqlSub  += string.Format("imsi like '%%{0}%%' and ", cq.imsi);              
            }

            if (!string.IsNullOrEmpty(cq.imei))
            {
                sqlSub += string.Format("imei like '%%{0}%%' and ", cq.imei);             
            }


            switch (cq.bwFlag)
            {
                case bwType.BWTYPE_WHITE:
                    {                
                        sqlSub += string.Format("bwFlag={0} and ", (int)cq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_BLACK:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)cq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_OTHER:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)cq.bwFlag);
                        break;
                    }
                default:
                    {
                        //不对该字段进行检索
                        break;
                    }
            }          

            if (string.Compare(cq.timeStart, cq.timeEnded) > 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_ST_EN_ERR], "DB", LogCategory.I);
                return (int)RC.TIME_ST_EN_ERR;
            }
            else
            {
                sqlSub += string.Format("time>='{0}' and time<='{1}' and ", cq.timeStart,cq.timeEnded);
            }

            if (sqlSub != "")
            {
                sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
            }
                                 
            if (cq.RmDupFlag == 1)
            {            
                   RmDupFlagString = "GROUP BY c.sn,c.imsi";
            }
            else
            {
                RmDupFlagString = "";
            }

            if (cq.topCount == 0)
            {
                topCountString = "";
            }
            else
            {
                topCountString = string.Format("Limit {0}", cq.topCount);
            }

            //处理imsi的检索               
            //string sql = string.Format("SELECT * FROM(SELECT c.*,d.name,d.sn from(SELECT a.imsi, a.imei, b.bwFlag, a.time, a.affDeviceId FROM(select imsi,imei,time,affDeviceId from capture where {0}) AS a INNER JOIN bwlist As b ON {1}) As c INNER JOIN device As d ON c.affDeviceId = d.id) As e {2}",
            //    sqlSub,sqlSub1,RmDupFlagString);


            //if (!string.IsNullOrEmpty(cq.imsi))
            //{
            //    //处理imsi的检索               
            //    sql = string.Format("SELECT a.imsi,a.imei,a.isdn,a.bsPwr,a.tmsi,b.bwFlag,a.time FROM(select * from capture where affDeviceId = {0} and imsi = '{1}' and time >= '{2}' and time <= '{3}' {4}) AS a INNER JOIN bwlist As b ON a.imsi = b.imsi and a.affDeviceId = b.affDeviceId and b.bwFlag = '{5}' ORDER BY a.time {6}", 
            //        cq.affDeviceId, cq.imsi, cq.timeStart, cq.timeEnded,RmDupFlagString, bwFlagString, topCountString);
            //}
            //else if (!string.IsNullOrEmpty(cq.imei))
            //{
            //    //处理imei的检索
            //}
            //else
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CAP_QUERY_INFO_ERR]);
            //    return (int)RC.CAP_QUERY_INFO_ERR;
            //}

            // 6 + 2 = 8项
            string sql = string.Format("Select * from (SELECT a.*, b.name,b.sn FROM(select imsi,imei,bwFlag,bsPwr,tmsi,time,affDeviceId from capture where {0}) AS a INNER JOIN device As b ON a.affDeviceId=b.id) As c {1}", sqlSub, RmDupFlagString);
            dt = new DataTable("capturequery");

            DataColumn column0 = new DataColumn("imsi",   System.Type.GetType("System.String"));
            DataColumn column1 = new DataColumn("imei",   System.Type.GetType("System.String"));
            DataColumn column2 = new DataColumn("name",   System.Type.GetType("System.String"));
            DataColumn column3 = new DataColumn("tmsi",   System.Type.GetType("System.String"));
            DataColumn column4 = new DataColumn("bsPwr",  System.Type.GetType("System.String"));
            DataColumn column5 = new DataColumn("time",   System.Type.GetType("System.String"));
            DataColumn column6 = new DataColumn("bwFlag", System.Type.GetType("System.String"));
            DataColumn column7 = new DataColumn("sn",     System.Type.GetType("System.String"));   

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["imsi"] = dr["imsi"].ToString();

                            if (!string.IsNullOrEmpty(dr["imei"].ToString()))
                            {
                                row["imei"] = dr["imei"].ToString();
                            }                            


                            row["name"] = dr["name"].ToString();
                            row["time"] = dr["time"].ToString();

                            if (!string.IsNullOrEmpty(dr["tmsi"].ToString()))
                            {
                                row["tmsi"] = dr["tmsi"].ToString();
                            }

                            row["bsPwr"] = dr["bsPwr"].ToString();

                            row["bwFlag"] = dr["bwFlag"].ToString();
                            row["sn"] = dr["sn"].ToString();

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 11-省市区操作

        /// <summary>
        /// 获取省
        /// </summary>
        /// <param name="province">返回省列表</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int db_getProvince_info(ref List<Province> province)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select provice_id,provice_name from j_position_provice");
            Province provinceVlues = new Province();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                provinceVlues.provice_id = dr.GetString(0);
                                provinceVlues.provice_name = dr.GetString(1);
                                province.Add(provinceVlues);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过省Id号获取对应的城市列表
        /// </summary>
        /// <param name="city">返回的省Id对应城市列表</param>
        /// <param name="provinceId">省Id</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int db_getCity_info(ref List<City> city, string provinceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(provinceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (provinceId.Length > 3)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select city_id,city_name from j_position_city where province_id='{0}'", provinceId);
            City cityVlues = new City();

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                cityVlues.city_id = dr.GetString(0);
                                cityVlues.city_name = dr.GetString(1);
                                city.Add(cityVlues);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过城市Id号获取对应的区/县列表
        /// </summary>
        /// <param name="distract">返回的城市Id号对应的区/县列表</param>
        /// <param name="cityId">城市Id</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int db_getDistract_info(ref List<Distract> distract, string cityId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(cityId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (cityId.Length > 12)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select county_id,county_name from j_position_county where city_id='{0}'", cityId);
            Distract countyVlues = new Distract();

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                countyVlues.county_id = dr.GetString(0);
                                countyVlues.county_name = dr.GetString(1);
                                distract.Add(countyVlues);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 12-ap_status操作

        /// <summary>
        /// 检查AP状态表记录是否存在
        /// </summary>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int ap_status_record_exist(int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from ap_status where affDeviceId = {0}", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到AP状态表中
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_status_record_insert(int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == ap_status_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into ap_status values(NULL,0,0,0,0,0,0,0,Now(),{0})", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到AP状态表中
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="apSts">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_status_record_update(int affDeviceId,strApStatus apSts)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_status_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(apSts.SCTP))
            {
                if (apSts.SCTP != "0" && apSts.SCTP != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("SCTP = {0},", apSts.SCTP);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(apSts.S1))
            {
                if (apSts.S1 != "0" && apSts.S1 != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("S1 = {0},", apSts.S1);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(apSts.GPS))
            {
                if (apSts.GPS != "0" && apSts.GPS != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("GPS = {0},", apSts.GPS);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(apSts.CELL))
            {
                if (apSts.CELL != "0" && apSts.CELL != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("CELL = {0},", apSts.CELL);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(apSts.SYNC))
            {
                if (apSts.SYNC != "0" && apSts.SYNC != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("SYNC = {0},", apSts.SYNC);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(apSts.LICENSE))
            {
                if (apSts.LICENSE != "0" && apSts.LICENSE != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("LICENSE = {0},", apSts.LICENSE);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(apSts.RADIO))
            {
                if (apSts.RADIO != "0" && apSts.RADIO != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("RADIO = {0},", apSts.RADIO);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(apSts.time))
            {
                try
                {
                    DateTime.Parse(apSts.time);
                    sqlSub += string.Format("time = '{0}',", apSts.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    sqlSub += string.Format("time = '{0}',",DateTime.Now.ToString());
                }                
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update ap_status set {0} where affDeviceId = {1}", sqlSub, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到AP状态表中
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="apSts">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_status_record_get_by_devid(int affDeviceId, ref strApStatus apSts)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_status_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            apSts = new strApStatus();
            string sql = string.Format("select * from ap_status where affDeviceId = {0}", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {                            
                            if (!string.IsNullOrEmpty(dr[1].ToString()))
                            {
                                apSts.SCTP = dr[1].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[2].ToString()))
                            {
                                apSts.S1 = dr[2].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[3].ToString()))
                            {
                                apSts.GPS = dr[3].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[4].ToString()))
                            {
                                apSts.CELL = dr[4].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[5].ToString()))
                            {
                                apSts.SYNC = dr[5].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[6].ToString()))
                            {
                                apSts.LICENSE = dr[6].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[7].ToString()))
                            {
                                apSts.RADIO = dr[7].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr[8].ToString()))
                            {
                                apSts.time = dr[8].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在AP状态表中删除指定的记录 
        /// </summary>  
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int ap_status_record_delete(int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }            

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_status_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from ap_status where affDeviceId = {0}", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 13-bwlist操作

        /// <summary>
        /// 检查imsi对应的黑白名单记录是否存在
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int bwlist_record_imsi_exist(string imsi, bwType bwFlag, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imsi))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imsi.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'black' and affDeviceId = {1}", imsi, affDeviceId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'white' and affDeviceId = {1}", imsi, affDeviceId);
            }
            else
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'other' and affDeviceId = {1}", imsi, affDeviceId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查imsi对应的黑白名单记录是否存在
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDomainId"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int bwlist_record_imsi_exist_domain(string imsi, bwType bwFlag, int affDomainId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imsi))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imsi.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'black' and affDomainId = {1}", imsi, affDomainId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'white' and affDomainId = {1}", imsi, affDomainId);
            }
            else
            {
                sql = string.Format("select count(*) from bwlist where imsi = '{0}' and bwFlag = 'other' and affDomainId = {1}", imsi, affDomainId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查imei对应的黑白名单记录是否存在
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int bwlist_record_imei_exist(string imei, bwType bwFlag, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imei.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'black' and affDeviceId = {1}", imei, affDeviceId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'white' and affDeviceId = {1}", imei, affDeviceId);
            }
            else
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'other' and affDeviceId = {1}", imei, affDeviceId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查imei对应的黑白名单记录是否存在
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDomainId"></param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int bwlist_record_imei_exist_domain(string imei, bwType bwFlag, int affDomainId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imei.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'black' and affDomainId = {1}", imei, affDomainId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'white' and affDomainId = {1}", imei, affDomainId);
            }
            else
            {
                sql = string.Format("select count(*) from bwlist where imei = '{0}' and bwFlag = 'other' and affDomainId = {1}", imei, affDomainId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到bwlist表中
        /// </summary>
        /// <param name="list">记录相关的信息</param>
        /// <param name="affDeviceId">所属的设备Id</param>
        /// <returns>
        ///   RC.NO_OPEN             ：数据库尚未打开
        ///   RC.PAR_NULL            ：参数为空
        ///   PAR_LEN_ERR            ：参数长度有误
        ///   RC.OP_FAIL             ：数据库操作失败 
        ///   EXIST                  ：记录已经存在
        ///   IMSI_IMEI_BOTH_NULL    ：IMSI和IMEI都为空
        ///   RC.DEV_NO_EXIST        ：设备不存在
        ///   RC.SUCCESS             ：成功 
        /// </returns>
        public int bwlist_record_insert(strBwList list, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(list.imsi) && string.IsNullOrEmpty(list.imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NULL], "DB", LogCategory.I);
                return (int)RC.IMSI_IMEI_BOTH_NULL;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            if (!string.IsNullOrEmpty(list.imsi))
            {
                //检查IMSI是否存在
                if ((int)RC.EXIST == bwlist_record_imsi_exist(list.imsi, list.bwFlag, affDeviceId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                    return (int)RC.EXIST;
                }
            }
            else
            {
                //检查IMEI是否存在
                if ((int)RC.EXIST == bwlist_record_imei_exist(list.imei, list.bwFlag, affDeviceId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                    return (int)RC.EXIST;
                }
            }


            //
            // 对于GSW和WCDMA，imsi和imei都为非空
            //
            //if (string.IsNullOrEmpty(list.imsi) && string.IsNullOrEmpty(list.imei))
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NULL]);
            //    return (int)RC.IMSI_IMEI_BOTH_NULL;
            //}
            //else if (!string.IsNullOrEmpty(list.imsi) && !string.IsNullOrEmpty(list.imei))
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NOTNULL]);
            //    return (int)RC.IMSI_IMEI_BOTH_NOTNULL;
            //}
            //else
            //{
            //    //要么imsi为空，要么imei为空
            //    if (string.IsNullOrEmpty(list.imsi))
            //    {
            //        if (list.imei.Length > 15)
            //        {
            //            Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR]);
            //            return (int)RC.PAR_LEN_ERR;
            //        }
            //    }
            //    else
            //    {
            //        if (list.imsi.Length > 15)
            //        {
            //            Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR]);
            //            return (int)RC.PAR_LEN_ERR;
            //        }
            //    }
            //}            

            string sqlSub = "";
            string sql = "";

            if (!string.IsNullOrEmpty(list.imsi))
            {
                sqlSub += string.Format("'{0}',", list.imsi);
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            if (!string.IsNullOrEmpty(list.imei))
            {
                sqlSub += string.Format("'{0}',", list.imei);
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }            

            if (list.bwFlag == bwType.BWTYPE_BLACK)
            {
                sqlSub += string.Format("'black',");
            }
            else if (list.bwFlag == bwType.BWTYPE_WHITE)
            {
                sqlSub += string.Format("'white',");
            }
            else
            {
                sqlSub += string.Format("'other',");
            }

            //list.rbStart
            if (!string.IsNullOrEmpty(list.rbStart))
            {
                if (list.rbStart.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", list.rbStart);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //list.rbEnd
            if (!string.IsNullOrEmpty(list.rbEnd))
            {
                if (list.rbEnd.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", list.rbEnd);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //list.time
            if (!string.IsNullOrEmpty(list.time))
            {
                try
                {
                    DateTime.Parse(list.time);
                    sqlSub += string.Format("'{0}',", list.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR] + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
            }
            else
            {
                sqlSub += string.Format("Now(),");
            }

            //list.des
            if (!string.IsNullOrEmpty(list.des))
            {
                if (list.des.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("'{0}',", list.des);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            if (!string.IsNullOrEmpty(list.affDeviceId))
            {
                sqlSub += string.Format("0,{0},NULL,", list.affDeviceId);
            }

            if (!string.IsNullOrEmpty(list.affDomainId))
            {
                sqlSub += string.Format("1,NULL,{0},", list.affDomainId);
            }
           

            ///
            /// linkFlag affDeviceId affDomainId
            /// 总是链接到DeviceId                
            /// 
            /// sqlSub += string.Format("0,{0},NULL,", affDeviceId);

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }           

            sql = string.Format("insert into bwlist values(NULL,{0})", sqlSub);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 2018-07-20
        /// 在白名单自学习中，将捕号记录保存成白名单
        /// </summary>
        /// <param name="cap">捕号记录的信息</param>
        /// <param name="affDeviceId">所属的设备Id</param>
        /// <returns>
        ///   RC.NO_OPEN             ：数据库尚未打开
        ///   RC.PAR_NULL            ：参数为空
        ///   PAR_LEN_ERR            ：参数长度有误
        ///   RC.OP_FAIL             ：数据库操作失败 
        ///   EXIST                  ：记录已经存在
        ///   IMSI_IMEI_BOTH_NULL    ：IMSI和IMEI都为空
        ///   RC.DEV_NO_EXIST        ：设备不存在
        ///   RC.SUCCESS             ：成功 
        /// </returns>
        public int bwlist_record_insert(strCapture cap, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(cap.imsi) && string.IsNullOrEmpty(cap.imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NULL], "DB", LogCategory.I);
                return (int)RC.IMSI_IMEI_BOTH_NULL;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            if (!string.IsNullOrEmpty(cap.imsi))
            {
                //检查IMSI是否存在(固定为白名单)
                if ((int)RC.EXIST == bwlist_record_imsi_exist(cap.imsi,bwType.BWTYPE_WHITE, affDeviceId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                    return (int)RC.EXIST;
                }
            }

            //
            // 对于GSW和WCDMA，imsi和imei都为非空
            //           

            string sql = "";
            string sqlSub = "";            

            if (!string.IsNullOrEmpty(cap.imsi))
            {
                sqlSub += string.Format("'{0}',", cap.imsi);
            }           

           
            //imei
            sqlSub += string.Format("NULL,");
            

            //固定为白名单
            sqlSub += string.Format("'white',");
            
            //rbStart
            sqlSub += string.Format("NULL,");
            
            //rbEnd
            sqlSub += string.Format("NULL,");
        
            //time
            if (!string.IsNullOrEmpty(cap.time))
            {
                try
                {
                    DateTime.Parse(cap.time);
                    sqlSub += string.Format("'{0}',", cap.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR] + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
            }
            else
            {
                sqlSub += string.Format("Now(),");
            }

            //des(固定为wSelfStudy)
            sqlSub += string.Format("'{0}',", "wSelfStudy");
       
            ///
            /// linkFlag affDeviceId affDomainId
            /// 总是链接到DeviceId                
            /// 
            sqlSub += string.Format("0,{0},NULL,", affDeviceId);

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }

            sql = string.Format("insert into bwlist values(NULL,{0})", sqlSub);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 插入记录到bwlist表中
        /// </summary>
        /// <param name="list">记录相关的信息</param>
        /// <param name="affDeviceId">所属的设备Id</param>
        /// <returns>
        ///   RC.NO_OPEN             ：数据库尚未打开
        ///   RC.PAR_NULL            ：参数为空
        ///   PAR_LEN_ERR            ：参数长度有误
        ///   RC.OP_FAIL             ：数据库操作失败 
        ///   EXIST                  ：记录已经存在
        ///   IMSI_IMEI_BOTH_NULL    ：IMSI和IMEI都为空
        ///   RC.DEV_NO_EXIST        ：设备不存在
        ///   RC.SUCCESS             ：成功 
        /// </returns>
        public int bwlist_record_insert_batch(List<strBwList> list, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (list.Count <= 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sqlSub = "";
            string sqlBig = "";

            foreach (strBwList str in list)
            {
                //(1)-id
                sqlSub = "NULL,";                

                //(2)-imsi
                if (!string.IsNullOrEmpty(str.imsi))
                {
                    if (str.imsi.Length > 15)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.imsi);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(3)-imei
                if (!string.IsNullOrEmpty(str.imei))
                {
                    if (str.imei.Length > 15)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.imei);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(4)-bwFlag
                switch (str.bwFlag)
                {
                    case bwType.BWTYPE_WHITE:
                        {
                            sqlSub += string.Format("{0},", (int)str.bwFlag);
                            break;
                        }
                    case bwType.BWTYPE_BLACK:
                        {
                            sqlSub += string.Format("{0},", (int)str.bwFlag);
                            break;
                        }
                    case bwType.BWTYPE_OTHER:
                        {
                            sqlSub += string.Format("{0},", (int)str.bwFlag);
                            break;
                        }
                    default:
                        {
                            //插入非数据库中定义的类型时，该字段显示为空值
                            sqlSub += string.Format("{0},", (int)str.bwFlag);
                            break;
                        }
                }

                //(5)-rbStart
                if (!string.IsNullOrEmpty(str.rbStart))
                {
                    if (str.rbStart.Length > 4)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", int.Parse(str.rbStart));
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(6)-rbEnd
                if (!string.IsNullOrEmpty(str.rbEnd))
                {
                    if (str.rbEnd.Length > 4)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", int.Parse(str.rbEnd));
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }              

                //(7)-time
                if (!string.IsNullOrEmpty(str.time))
                {
                    if (str.time.Length > 19)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.time);
                    }
                }
                else
                {
                    //sqlSub += string.Format("NULL,");
                    sqlSub += string.Format("'{0}',",DateTime.Now.ToString());
                }

                //(8)-des
                if (!string.IsNullOrEmpty(str.des))
                {
                    if (str.des.Length > 128)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.des);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");                    
                }


                //(9)-linkFlag
                if (!string.IsNullOrEmpty(str.linkFlag))
                {
                    if (str.linkFlag.Length > 1)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", int.Parse(str.linkFlag));
                    }
                }
                else
                {
                    sqlSub += string.Format("0,");
                }


                //(10)-affDeviceId
                if (!string.IsNullOrEmpty(str.affDeviceId))
                {
                    if (str.affDeviceId.Length > 10)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", str.affDeviceId);
                    }
                }
                else
                {
                    sqlSub += string.Format("{0},",affDeviceId);
                }

                //(11)-affDomainId
                if (!string.IsNullOrEmpty(str.affDomainId))
                {
                    if (str.affDomainId.Length > 10)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("{0},", str.affDomainId);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }


                if (sqlSub != "")
                {
                    //去掉最后一个字符
                    sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
                    sqlSub = string.Format("({0}),", sqlSub);

                    sqlBig += sqlSub;
                }
            }

            if (sqlBig != "")
            {
                //去掉最后一个字符
                sqlBig = sqlBig.Remove(sqlBig.Length - 1, 1);
            }

            string sql = string.Format("insert into bwlist values{0}", sqlBig);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

           return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 插入记录到bwlist表中，属于域
        /// </summary>
        /// <param name="list">记录相关的信息</param>
        /// <param name="affDomainId">所属的域Id</param>
        /// <returns>
        ///   RC.NO_OPEN             ：数据库尚未打开
        ///   RC.PAR_NULL            ：参数为空
        ///   PAR_LEN_ERR            ：参数长度有误
        ///   RC.OP_FAIL             ：数据库操作失败 
        ///   EXIST                  ：记录已经存在
        ///   IMSI_IMEI_BOTH_NULL    ：IMSI和IMEI都为空
        ///   RC.DOMAIN_NO_EXIST     ：域不存在
        ///   RC.SUCCESS             ：成功 
        /// </returns>
        public int bwlist_record_insert_affdomainid(strBwList list, int affDomainId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(list.imsi) && string.IsNullOrEmpty(list.imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NULL], "DB", LogCategory.I);
                return (int)RC.IMSI_IMEI_BOTH_NULL;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DOMAIN_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DOMAIN_NO_EXIST;
            }

            if (!string.IsNullOrEmpty(list.imsi))
            {
                //检查IMSI是否存在
                if ((int)RC.EXIST == bwlist_record_imsi_exist_domain(list.imsi, list.bwFlag, affDomainId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                    return (int)RC.EXIST;
                }
            }
            else
            {
                //检查IMEI是否存在
                if ((int)RC.EXIST == bwlist_record_imei_exist_domain(list.imei, list.bwFlag, affDomainId))
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                    return (int)RC.EXIST;
                }
            }


            //
            // 对于GSW和WCDMA，imsi和imei都为非空
            //
            //if (string.IsNullOrEmpty(list.imsi) && string.IsNullOrEmpty(list.imei))
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NULL]);
            //    return (int)RC.IMSI_IMEI_BOTH_NULL;
            //}
            //else if (!string.IsNullOrEmpty(list.imsi) && !string.IsNullOrEmpty(list.imei))
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.IMSI_IMEI_BOTH_NOTNULL]);
            //    return (int)RC.IMSI_IMEI_BOTH_NOTNULL;
            //}
            //else
            //{
            //    //要么imsi为空，要么imei为空
            //    if (string.IsNullOrEmpty(list.imsi))
            //    {
            //        if (list.imei.Length > 15)
            //        {
            //            Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR]);
            //            return (int)RC.PAR_LEN_ERR;
            //        }
            //    }
            //    else
            //    {
            //        if (list.imsi.Length > 15)
            //        {
            //            Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR]);
            //            return (int)RC.PAR_LEN_ERR;
            //        }
            //    }
            //}            

            string sqlSub = "";
            string sql = "";

            if (!string.IsNullOrEmpty(list.imsi))
            {
                sqlSub += string.Format("'{0}',", list.imsi);
            }
            else
            {
                sqlSub += string.Format("'NULL',");
            }

            if (!string.IsNullOrEmpty(list.imei))
            {
                sqlSub += string.Format("'{0}',", list.imei);
            }
            else
            {
                sqlSub += string.Format("'NULL',");
            }

            if (list.bwFlag == bwType.BWTYPE_BLACK)
            {
                sqlSub += string.Format("'black',");
            }
            else if (list.bwFlag == bwType.BWTYPE_WHITE)
            {
                sqlSub += string.Format("'white',");
            }
            else
            {
                sqlSub += string.Format("'other',");
            }

            //list.rbStart
            if (!string.IsNullOrEmpty(list.rbStart))
            {
                if (list.rbStart.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", list.rbStart);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //list.rbEnd
            if (!string.IsNullOrEmpty(list.rbEnd))
            {
                if (list.rbEnd.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("{0},", list.rbEnd);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            //list.time
            if (!string.IsNullOrEmpty(list.time))
            {
                try
                {
                    DateTime.Parse(list.time);
                    sqlSub += string.Format("'{0}',", list.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR] + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
            }
            else
            {
                sqlSub += string.Format("Now(),");
            }

            //list.des
            if (!string.IsNullOrEmpty(list.des))
            {
                if (list.des.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("'{0}',", list.des);
                }
            }
            else
            {
                sqlSub += string.Format("NULL,");
            }

            if (!string.IsNullOrEmpty(list.affDeviceId))
            {
                sqlSub += string.Format("0,{0},NULL,", list.affDeviceId);
            }

            if (!string.IsNullOrEmpty(list.affDomainId))
            {
                sqlSub += string.Format("1,NULL,{0},", list.affDomainId);
            }


            ///
            /// linkFlag affDeviceId affDomainId
            /// 总是链接到DeviceId                
            /// 
            /// sqlSub += string.Format("0,{0},NULL,", affDeviceId);

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }

            sql = string.Format("insert into bwlist values(NULL,{0})", sqlSub);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在bwlist表中删除指定的记录 
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int bwlist_record_imsi_delete(string imsi, bwType bwFlag, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imsi))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imsi.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == bwlist_record_imsi_exist(imsi, bwFlag, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'black' and affDeviceId = {1}", imsi, affDeviceId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'white' and affDeviceId = {1}", imsi, affDeviceId);
            }
            else
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'other' and affDeviceId = {1}", imsi, affDeviceId);
            }
            
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }        

        /// <summary>
        /// 在bwlist表中删除指定的记录 
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int bwlist_record_imei_delete(string imei, bwType bwFlag, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imei))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imei.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == bwlist_record_imei_exist(imei, bwFlag, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("delete from bwlist where imei = '{0}' and bwFlag = 'black' and affDeviceId = {1}", imei, affDeviceId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("delete from bwlist where imei = '{0}' and bwFlag = 'white' and affDeviceId = {1}", imei, affDeviceId);
            }
            else
            {
                sql = string.Format("delete from bwlist where imei = '{0}' and bwFlag = 'other' and affDeviceId = {1}", imei, affDeviceId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在bwlist表中删除指定的记录 
        /// </summary>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int bwlist_record_bwflag_delete(bwType bwFlag, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }            

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }            

            string sql = "";
            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("delete from bwlist where bwFlag = 'black' and affDeviceId = {0}", affDeviceId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("delete from bwlist where bwFlag = 'white' and affDeviceId = {0}", affDeviceId);
            }
            else
            {
                sql = string.Format("delete from bwlist where bwFlag = 'other' and affDeviceId = {0}", affDeviceId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在bwlist表中删除指定的记录 
        /// </summary>
        /// <param name="imsi"></param>
        /// <param name="bwFlag"></param>
        /// <param name="affDomainId"></param>
        /// <returns>
        ///   RC.NO_OPEN         ：数据库尚未打开
        ///   RC.PAR_NULL        ：参数为空
        ///   PAR_LEN_ERR        ：参数长度有误
        ///   RC.OP_FAIL         ：数据库操作失败 
        ///   RC.NO_EXIST        ：记录不存在
        ///   RC.DOMAIN_NO_EXIST ：域不存在
        ///   RC.SUCCESS         ：成功
        /// </returns>
        public int bwlist_record_imsi_delete_affdomainid(string imsi, bwType bwFlag, int affDomainId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(imsi))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (imsi.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查域是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DOMAIN_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DOMAIN_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == bwlist_record_imsi_exist_domain(imsi, bwFlag, affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = "";

            if (bwFlag == bwType.BWTYPE_BLACK)
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'black' and affDomainId = {1}", imsi, affDomainId);
            }
            else if (bwFlag == bwType.BWTYPE_WHITE)
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'white' and affDomainId = {1}", imsi, affDomainId);
            }
            else
            {
                sql = string.Format("delete from bwlist where imsi = '{0}' and bwFlag = 'other' and affDomainId = {1}", imsi, affDomainId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在bwlist表中删除指定的记录 
        /// </summary>
        /// <param name="affDeviceId"></param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败         
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int bwlist_record_delete(int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            //if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            //{
            //    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST]);
            //    return (int)RC.DEV_NO_EXIST;
            //}

            string sql = sql = string.Format("delete from bwlist where affDeviceId = {0}", affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取bwlist表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,imsi,imei,bwFlag,rbStart,rbEnd,time,des,
        /// linkFlag,affDeviceId,afDomainId,failDomainIdSet
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int bwlist_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("bwlist");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "imsi";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "imei";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "bwFlag";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.Int32");
            column4.ColumnName = "rbStart";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.Int32");
            column5.ColumnName = "rbEnd";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "time";

            DataColumn column7 = new DataColumn();
            column7.DataType = System.Type.GetType("System.String");
            column7.ColumnName = "des";

            DataColumn column8 = new DataColumn();
            column8.DataType = System.Type.GetType("System.Int32");
            column8.ColumnName = "linkFlag";

            DataColumn column9 = new DataColumn();
            column9.DataType = System.Type.GetType("System.Int32");
            column9.ColumnName = "affDeviceId";

            DataColumn column10 = new DataColumn();
            column10.DataType = System.Type.GetType("System.Int32");
            column10.ColumnName = "affDomainId";

            DataColumn column11 = new DataColumn();
            column11.DataType = System.Type.GetType("System.String");
            column11.ColumnName = "failDomainIdSet";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);

            string sql = string.Format("select * from bwlist");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToUInt32(dr["id"]);
                            row["imsi"] = dr["imsi"].ToString();
                            row["imei"] = dr["imei"].ToString();
                            row["bwFlag"] = dr["bwFlag"].ToString();

                            if (!string.IsNullOrEmpty(dr["rbStart"].ToString()))
                            {
                                row["rbStart"] = Convert.ToInt32(dr["rbStart"]);
                            }

                            if (!string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                            {
                                row["rbEnd"] = Convert.ToInt32(dr["rbEnd"]);
                            }

                            row["time"] = dr["time"].ToString();
                            row["des"] = dr["des"].ToString();

                            if (!string.IsNullOrEmpty(dr["linkFlag"].ToString()))
                            {
                                row["linkFlag"] = Convert.ToInt32(dr["linkFlag"]);
                            }

                            if (!string.IsNullOrEmpty(dr["affDeviceId"].ToString()))
                            {
                                row["affDeviceId"] = Convert.ToInt32(dr["affDeviceId"]);
                            }

                            if (!string.IsNullOrEmpty(dr["affDomainId"].ToString()))
                            {
                                row["affDomainId"] = Convert.ToInt32(dr["affDomainId"]);
                            }

                            row["failDomainIdSet"] = dr["failDomainIdSet"].ToString();
                            
                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过所属设备ID获取bwlist表中的各条记录
        /// </summary>
        /// <param name="dt"></param>      
        /// <param name="affDeviceId">所属设备ID</param>      
        /// <param name="bq">过滤的各种条件</param>      
        /// 返回的DataTable，包含的列为：imsi,imei,bwFlag,rbStart,rbEnd,time,des
        /// </param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int bwlist_record_entity_get(ref DataTable dt,int affDeviceId,strBwQuery bq)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            dt = new DataTable("bwlist");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.String");
            column0.ColumnName = "imsi";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "imei";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "bwFlag";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.Int32");
            column3.ColumnName = "rbStart";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.Int32");
            column4.ColumnName = "rbEnd";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "time";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "des";            

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);

            string sql = "";
            string sqlSub = "";

            if (!string.IsNullOrEmpty(bq.imsi))
            {
                sqlSub += string.Format("imsi like '%%{0}%%' and ", bq.imsi);
            }

            if (!string.IsNullOrEmpty(bq.imei))
            {
                sqlSub += string.Format("imei like '%%{0}%%' and ", bq.imei);
            }

            switch (bq.bwFlag)
            {
                case bwType.BWTYPE_WHITE:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_BLACK:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_OTHER:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                default:
                    {
                        //不对该字段进行检索
                        break;
                    }
            }

            if (string.Compare(bq.timeStart, bq.timeEnded) > 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_ST_EN_ERR], "DB", LogCategory.I);
                return (int)RC.TIME_ST_EN_ERR;
            }
            else
            {
                sqlSub += string.Format("time>='{0}' and time<='{1}' and ", bq.timeStart, bq.timeEnded);
            }

            if (!string.IsNullOrEmpty(bq.des))
            {
                sqlSub += string.Format("des like '%%{0}%%' and ", bq.des);
            }

            if (sqlSub != "")
            {
                sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
            }

            if (sqlSub != "")
            {
                sql = string.Format("select imsi,imei,bwFlag,rbStart,rbEnd,time,des from bwlist where {0} and affDeviceId = {1}", sqlSub,affDeviceId);
            }
            else
            {
                sql = string.Format("select imsi,imei,bwFlag,rbStart,rbEnd,time,des from bwlist where affDeviceId = {0}", affDeviceId);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();                          
                            
                            row["imsi"] = dr["imsi"].ToString();
                            row["imei"] = dr["imei"].ToString();
                            row["bwFlag"] = dr["bwFlag"].ToString();

                            if (!string.IsNullOrEmpty(dr["rbStart"].ToString()))
                            {
                                row["rbStart"] = Convert.ToInt32(dr["rbStart"]);
                            }

                            if (!string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                            {
                                row["rbEnd"] = Convert.ToInt32(dr["rbEnd"]);
                            }

                            row["time"] = dr["time"].ToString();
                            row["des"] = dr["des"].ToString();                                                      

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过黑白名单标识和所属设备ID，获取对应的IMSI列表
        /// </summary>
        /// <param name="bwFlag"></param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="listIMSI"></param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int bwlist_record_md5sum_get(bwType bwFlag,int affDeviceId,ref List<string> listIMSI)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            if (bwFlag != bwType.BWTYPE_BLACK && bwFlag != bwType.BWTYPE_WHITE)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }

            string tmp = "";
            listIMSI = new List<string>();
            string sql = string.Format("select imsi,imei,bwFlag,rbStart,rbEnd from bwlist where bwFlag = {0} and affDeviceId = {1}", (int)bwFlag,affDeviceId);
        
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (bwFlag == bwType.BWTYPE_BLACK)
                            {
                                if (!string.IsNullOrEmpty(dr["imsi"].ToString()) &&
                                    !string.IsNullOrEmpty(dr["rbStart"].ToString()) &&
                                    !string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                                {
                                    tmp = string.Format("{0},{1},{2}", dr["imsi"].ToString(), dr["rbStart"].ToString(), dr["rbEnd"].ToString());
                                    listIMSI.Add(tmp);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(dr["imsi"].ToString()))
                                {
                                    tmp = string.Format("{0}", dr["imsi"].ToString());
                                    listIMSI.Add(tmp);
                                }
                            }                   
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过所属设备ID列表获取bwlist表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：imsi,imei,bwFlag,rbStart,rbEnd,time,des
        /// </param>
        /// <param name="listAffDeviceId">所属设备ID列表</param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   DEV_NO_EXIST ：设备不存在
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int bwlist_record_entity_get(ref DataTable dt, List<int> listAffDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string devList = "";

            foreach (int dev in listAffDeviceId)
            {
                //检查设备是否存在
                if ((int)RC.EXIST == device_record_exist(dev))
                {
                    devList += string.Format("affDeviceId = {0} or ", dev);
                }
            }

            if (devList != "")
            {
                devList = devList.Remove(devList.Length - 3, 3);
            }
            else
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            dt = new DataTable("bwlist");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.String");
            column0.ColumnName = "imsi";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "imei";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "bwFlag";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.Int32");
            column3.ColumnName = "rbStart";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.Int32");
            column4.ColumnName = "rbEnd";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "time";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "des";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);

            string sql = string.Format("select imsi,imei,bwFlag,rbStart,rbEnd,time,des from bwlist where {0} ", devList);
            //string sql = string.Format("select a.*,b.name from (select * from bwlist where {0}) As a INNER JOIN device As b ON a.affDeviceId = b.id", devList);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["imsi"] = dr["imsi"].ToString();
                            row["imei"] = dr["imei"].ToString();
                            row["bwFlag"] = dr["bwFlag"].ToString();

                            if (!string.IsNullOrEmpty(dr["rbStart"].ToString()))
                            {
                                row["rbStart"] = Convert.ToInt32(dr["rbStart"]);
                            }

                            if (!string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                            {
                                row["rbEnd"] = Convert.ToInt32(dr["rbEnd"]);
                            }

                            row["time"] = dr["time"].ToString();
                            row["des"] = dr["des"].ToString();                          

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过所属设备ID列表+所属域ID获取bwlist表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：imsi,imei,bwFlag,rbStart,rbEnd,time,des
        /// </param>
        /// <param name="listAffDeviceId">所属设备ID列表</param>
        /// <param name="affDomainId">所属域ID</param>
        /// <param name="bq">过滤的各种条件</param>      
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   DEV_NO_EXIST    ：设备不存在
        ///   DOMAIN_NO_EXIST ：域不存在
        ///   RC.SUCCESS      ：成功 
        /// </returns>
        public int bwlist_record_entity_get(ref DataTable dt, List<int> listAffDeviceId,int affDomainId, strBwQuery bq)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == domain_record_exist(affDomainId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DOMAIN_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DOMAIN_NO_EXIST;
            }

            string devDomainList = "";
            foreach (int dev in listAffDeviceId)
            {
                //检查设备是否存在
                if ((int)RC.EXIST == device_record_exist(dev))
                {
                    devDomainList += string.Format("affDeviceId = {0} or ", dev);
                }
            }

            if (devDomainList != "")
            {
                devDomainList = devDomainList.Remove(devDomainList.Length - 3, 3);
            }
            else
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            devDomainList += string.Format("or affDomainId = {0} ", affDomainId);

            dt = new DataTable("bwlist");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.String");
            column0.ColumnName = "imsi";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "imei";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "bwFlag";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.Int32");
            column3.ColumnName = "rbStart";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.Int32");
            column4.ColumnName = "rbEnd";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "time";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "des";           

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);

            string sql = "";
            string sqlSub = "";

            if (!string.IsNullOrEmpty(bq.imsi))
            {
                sqlSub += string.Format("imsi like '%%{0}%%' and ", bq.imsi);
            }

            if (!string.IsNullOrEmpty(bq.imei))
            {
                sqlSub += string.Format("imei like '%%{0}%%' and ", bq.imei);
            }

            switch (bq.bwFlag)
            {
                case bwType.BWTYPE_WHITE:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_BLACK:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                case bwType.BWTYPE_OTHER:
                    {
                        sqlSub += string.Format("bwFlag={0} and ", (int)bq.bwFlag);
                        break;
                    }
                default:
                    {
                        //不对该字段进行检索
                        break;
                    }
            }

            if (string.Compare(bq.timeStart, bq.timeEnded) > 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_ST_EN_ERR], "DB", LogCategory.I);
                return (int)RC.TIME_ST_EN_ERR;
            }
            else
            {
                sqlSub += string.Format("time>='{0}' and time<='{1}' and ", bq.timeStart, bq.timeEnded);
            }

            if (!string.IsNullOrEmpty(bq.des))
            {
                sqlSub += string.Format("des like '%%{0}%%' and ", bq.des);
            }

            if (sqlSub != "")
            {
                sqlSub = sqlSub.Remove(sqlSub.Length - 4, 4);
            }

            //string sql = string.Format("SELECT c.*,d.id,d.parentId,d.nameFullPath from (select a.*,b.name,b.affDomainId from (select imsi,imei,bwFlag,rbStart,rbEnd,time,des,affDeviceId from bwlist where {0}) As a INNER JOIN device As b ON a.affDeviceId = b.id) As c INNER JOIN domain As d ON c.affDomainId = d.id", devDomainList);
            //string sql = string.Format("select a.*,b.name from (select * from bwlist where {0}) As a INNER JOIN device As b ON a.affDeviceId = b.id", devList);

            //string sql = string.Format("select * from bwlist where {0} group by imsi,imei", devDomainList);

            if (sqlSub != "")
            {
                sql = string.Format("select * from bwlist where ({0}) and ({1}) group by imsi,imei", sqlSub,devDomainList);
            }
            else
            {
                sql = string.Format("select * from bwlist where {0} group by imsi,imei", devDomainList);
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["imsi"] = dr["imsi"].ToString();
                            row["imei"] = dr["imei"].ToString();
                            row["bwFlag"] = dr["bwFlag"].ToString();

                            if (!string.IsNullOrEmpty(dr["rbStart"].ToString()))
                            {
                                row["rbStart"] = Convert.ToInt32(dr["rbStart"]);
                            }

                            if (!string.IsNullOrEmpty(dr["rbEnd"].ToString()))
                            {
                                row["rbEnd"] = Convert.ToInt32(dr["rbEnd"]);
                            }

                            row["time"] = dr["time"].ToString();
                            row["des"] = dr["des"].ToString();                           

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 14-ap_general_para操作

        /// <summary>
        /// 检查AP通用参数表记录是否存在
        /// </summary>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int ap_general_para_record_exist(int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from ap_general_para where affDeviceId = {0}", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到AP通用参数表中
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_general_para_record_insert(int affDeviceId,string mode)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == ap_general_para_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            // string sql = string.Format("insert into ap_general_para values(NULL,'{0}',NULL,NULL,NULL,
            // NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,
            // NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,Now(),{1})", mode,affDeviceId);

            // 2018-07-23
            string sql = string.Format("insert into ap_general_para(id,mode,time,affDeviceId) values(NULL,'{0}',Now(),{1})", mode, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到AP通用参数表中
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="apGP">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_general_para_record_update(int affDeviceId, strApGenPara apGP)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_general_para_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(apGP.mode))
            {
                if (apGP.mode.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    devMode dm = get_device_mode(apGP.mode);

                    //if ((apGP.mode != "GSM") &&
                    //    (apGP.mode != "TD-SCDMA") &&
                    //    (apGP.mode != "WCDMA") &&
                    //    (apGP.mode != "LTE-TDD") &&
                    //    (apGP.mode != "LTE-FDD"))

                    if(dm == devMode.MODE_UNKNOWN)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + "mode不支持.", "DB", LogCategory.I);
                        return (int)RC.PAR_FMT_ERR;
                    }
                    else
                    {
                        sqlSub += string.Format("mode = '{0}',", apGP.mode);
                    }
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(apGP.primaryplmn))
            {
                if (apGP.primaryplmn.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("primaryplmn = '{0}',", apGP.primaryplmn);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(apGP.earfcndl))
            {
                if (apGP.earfcndl.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("earfcndl = '{0}',", apGP.earfcndl);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(apGP.earfcnul))
            {
                if (apGP.earfcnul.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("earfcnul = '{0}',", apGP.earfcnul);
                }
            }

            //(5)  2018-06-26
            if (!string.IsNullOrEmpty(apGP.cellid))
            {
                if (apGP.cellid.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("cellid = '{0}',", apGP.cellid);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(apGP.pci))
            {
                if (apGP.pci.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("pci = '{0}',", apGP.pci);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(apGP.bandwidth))
            {
                if (apGP.bandwidth.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bandwidth = '{0}',", apGP.bandwidth);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(apGP.tac))
            {
                try
                {
                    UInt16.Parse(apGP.tac);
                    sqlSub += string.Format("tac = {0},", apGP.tac);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR,ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }               
            }

            //(9)
            if (!string.IsNullOrEmpty(apGP.txpower))
            {
                if (apGP.txpower.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("txpower = '{0}',", apGP.txpower);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(apGP.periodtac))
            {
                try
                {
                    int.Parse(apGP.periodtac);
                    sqlSub += string.Format("periodtac = {0},", apGP.periodtac);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(11)
            if (!string.IsNullOrEmpty(apGP.manualfreq))
            {
                try
                {
                    int.Parse(apGP.manualfreq);
                    sqlSub += string.Format("manualfreq = {0},", apGP.manualfreq);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(12)
            if (!string.IsNullOrEmpty(apGP.bootMode))
            {
                try
                {
                    int.Parse(apGP.bootMode);
                    sqlSub += string.Format("bootMode = {0},", apGP.bootMode);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(13)
            if (!string.IsNullOrEmpty(apGP.Earfcnlist))
            {
                if (apGP.Earfcnlist.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    if (false == check_id_set(apGP.Earfcnlist))
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + "Earfcnlist格式有误", "DB", LogCategory.I);
                        return (int)RC.PAR_FMT_ERR;
                    }
                    else
                    {
                        sqlSub += string.Format("Earfcnlist = '{0}',", apGP.Earfcnlist);
                    }
                }
            }

            //(14)
            if (!string.IsNullOrEmpty(apGP.Bandoffset))
            {
                if (apGP.Bandoffset.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("Bandoffset = '{0}',", apGP.Bandoffset);
                }
            }

            //(15)
            if (!string.IsNullOrEmpty(apGP.NTP))
            {
                if (apGP.NTP.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    try
                    {
                        IPAddress.Parse(apGP.NTP);
                        sqlSub += string.Format("NTP = '{0}',", apGP.NTP);
                    }
                    catch(Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + ",NTP格式有误," + ee.Message, "DB", LogCategory.I);
                        return (int)RC.PAR_FMT_ERR;
                    }                    
                }
            }

            //(16)
            if (!string.IsNullOrEmpty(apGP.ntppri))
            {
                try
                {
                    int.Parse(apGP.ntppri);
                    sqlSub += string.Format("ntppri = {0},", apGP.ntppri);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(17)
            if (!string.IsNullOrEmpty(apGP.source))
            {
                try
                {
                    int.Parse(apGP.source);
                    sqlSub += string.Format("source = {0},", apGP.source);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(18)
            if (!string.IsNullOrEmpty(apGP.ManualEnable))
            {
                try
                {
                    int.Parse(apGP.ManualEnable);
                    sqlSub += string.Format("ManualEnable = {0},", apGP.ManualEnable);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(19)
            if (!string.IsNullOrEmpty(apGP.ManualEarfcn))
            {
                try
                {
                    int.Parse(apGP.ManualEarfcn);
                    sqlSub += string.Format("ManualEarfcn = {0},", apGP.ManualEarfcn);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(20)
            if (!string.IsNullOrEmpty(apGP.ManualPci))
            {
                try
                {
                    int.Parse(apGP.ManualPci);
                    sqlSub += string.Format("ManualPci = {0},", apGP.ManualPci);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(21)
            if (!string.IsNullOrEmpty(apGP.ManualBw))
            {
                try
                {
                    int.Parse(apGP.ManualBw);
                    sqlSub += string.Format("ManualBw = {0},", apGP.ManualBw);
                }
                catch (Exception ex)
                {
                    Logger.Trace(LogInfoType.EROR, ex.Message + dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(22)
            if (!string.IsNullOrEmpty(apGP.gpsConfig))
            {
                if ((apGP.gpsConfig != "0") && (apGP.gpsConfig != "1"))
                {
                    Logger.Trace(LogInfoType.EROR,dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("gpsConfig = {0},", apGP.gpsConfig);
                }              
            }



            // 2108-07-23
            if (!string.IsNullOrEmpty(apGP.otherplmn))
            {
                if (apGP.otherplmn.Length > 256)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    if (false == check_id_set(apGP.otherplmn))
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + "otherplmn格式有误", "DB", LogCategory.I);
                        return (int)RC.PAR_FMT_ERR;
                    }
                    else
                    {
                        sqlSub += string.Format("otherplmn = '{0}',", apGP.otherplmn);
                    }
                }
            }

            if (!string.IsNullOrEmpty(apGP.periodFreq))
            {
                if (apGP.periodFreq.Length > 256)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("periodFreq = '{0}',", apGP.periodFreq);
                }
            }

            if (!string.IsNullOrEmpty(apGP.res1))
            {
                if (apGP.res1.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res1 = '{0}',", apGP.res1);
                }
            }
         

            if (!string.IsNullOrEmpty(apGP.res2))
            {
                if (apGP.res2.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res2 = '{0}',", apGP.res2);
                }
            }

            if (!string.IsNullOrEmpty(apGP.res3))
            {
                if (apGP.res3.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res3 = '{0}',", apGP.res3);
                }
            }

            //(23,24)
            if (!string.IsNullOrEmpty(apGP.activeTime1Start)  && !string.IsNullOrEmpty(apGP.activeTime1Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(apGP.activeTime1Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(apGP.activeTime1Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime1Start = '{0}',activeTime1Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(25,26)
            if (!string.IsNullOrEmpty(apGP.activeTime2Start) && !string.IsNullOrEmpty(apGP.activeTime2Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(apGP.activeTime2Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(apGP.activeTime2Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime2Start = '{0}',activeTime2Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(27,28)
            if (!string.IsNullOrEmpty(apGP.activeTime3Start) && !string.IsNullOrEmpty(apGP.activeTime3Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(apGP.activeTime3Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(apGP.activeTime3Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime3Start = '{0}',activeTime3Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(29,30)
            if (!string.IsNullOrEmpty(apGP.activeTime4Start) && !string.IsNullOrEmpty(apGP.activeTime4Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(apGP.activeTime4Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(apGP.activeTime4Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime4Start = '{0}',activeTime4Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(31)
            if (!string.IsNullOrEmpty(apGP.time))
            {
                try
                {
                    DateTime.Parse(apGP.time);
                    sqlSub += string.Format("time = '{0}',", apGP.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    sqlSub += string.Format("time = '{0}',", DateTime.Now.ToString());
                }
            }
            else
            {
                sqlSub += string.Format("time = '{0}',", DateTime.Now.ToString());
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update ap_general_para set {0} where affDeviceId = {1}", sqlSub, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取AP通用参数记录
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="apGP">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_general_para_record_get_by_devid(int affDeviceId, ref strApGenPara apGP)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_general_para_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            apGP = new strApGenPara();
            string sql = string.Format("select * from ap_general_para where affDeviceId = {0}",affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["mode"].ToString()))
                            {
                                apGP.mode = dr["mode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["primaryplmn"].ToString()))
                            {
                                apGP.primaryplmn = dr["primaryplmn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["earfcndl"].ToString()))
                            {
                                apGP.earfcndl = dr["earfcndl"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["earfcnul"].ToString()))
                            {
                                apGP.earfcnul = dr["earfcnul"].ToString();
                            }

                            // 2018-06-26
                            if (!string.IsNullOrEmpty(dr["cellid"].ToString()))
                            {
                                apGP.cellid = dr["cellid"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["pci"].ToString()))
                            {
                                apGP.pci = dr["pci"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bandwidth"].ToString()))
                            {
                                apGP.bandwidth = dr["bandwidth"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["tac"].ToString()))
                            {
                                apGP.tac = dr["tac"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["txpower"].ToString()))
                            {
                                apGP.txpower = dr["txpower"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["periodtac"].ToString()))
                            {
                                apGP.periodtac = dr["periodtac"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["manualfreq"].ToString()))
                            {
                                apGP.manualfreq = dr["manualfreq"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bootMode"].ToString()))
                            {
                                apGP.bootMode = dr["bootMode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["Earfcnlist"].ToString()))
                            {
                                apGP.Earfcnlist = dr["Earfcnlist"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["Bandoffset"].ToString()))
                            {
                                apGP.Bandoffset = dr["Bandoffset"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["NTP"].ToString()))
                            {
                                apGP.NTP = dr["NTP"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["ntppri"].ToString()))
                            {
                                apGP.ntppri = dr["ntppri"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["source"].ToString()))
                            {
                                apGP.source = dr["source"].ToString();
                            }
                            else
                            {
                                apGP.source = "0";
                            }

                            if (!string.IsNullOrEmpty(dr["ManualEnable"].ToString()))
                            {
                                apGP.ManualEnable = dr["ManualEnable"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["ManualEarfcn"].ToString()))
                            {
                                apGP.ManualEarfcn = dr["ManualEarfcn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["ManualPci"].ToString()))
                            {
                                apGP.ManualPci = dr["ManualPci"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["ManualBw"].ToString()))
                            {
                                apGP.ManualBw = dr["ManualBw"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["gpsConfig"].ToString()))
                            {
                                apGP.gpsConfig = dr["gpsConfig"].ToString();
                            }
                            else
                            {
                                apGP.gpsConfig = "0";
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["otherplmn"].ToString()))
                            {
                                apGP.otherplmn = dr["otherplmn"].ToString();
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["periodFreq"].ToString()))
                            {
                                apGP.periodFreq = dr["periodFreq"].ToString();
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["res1"].ToString()))
                            {
                                apGP.res1 = dr["res1"].ToString();
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["res2"].ToString()))
                            {
                                apGP.res2 = dr["res2"].ToString();
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["res3"].ToString()))
                            {
                                apGP.res3 = dr["res3"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime1Start"].ToString()))
                            {
                                apGP.activeTime1Start = dr["activeTime1Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime1Ended"].ToString()))
                            {
                                apGP.activeTime1Ended = dr["activeTime1Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime2Start"].ToString()))
                            {
                                apGP.activeTime2Start = dr["activeTime2Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime2Ended"].ToString()))
                            {
                                apGP.activeTime2Ended = dr["activeTime2Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime3Start"].ToString()))
                            {
                                apGP.activeTime3Start = dr["activeTime3Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime3Ended"].ToString()))
                            {
                                apGP.activeTime3Ended = dr["activeTime3Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime4Start"].ToString()))
                            {
                                apGP.activeTime4Start = dr["activeTime4Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime4Ended"].ToString()))
                            {
                                apGP.activeTime4Ended = dr["activeTime4Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["time"].ToString()))
                            {
                                apGP.time = dr["time"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取AP通用参数string
        /// </summary>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="apGP">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int ap_general_para_string_get_by_devid(int affDeviceId, ref string genParaString)
        {
            int rtv = -1;
            strApGenPara apGP = new strApGenPara();

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            rtv = ap_general_para_record_get_by_devid(affDeviceId, ref apGP);
            if ((int)RC.SUCCESS != rtv)
            {
                return rtv;
            }

            genParaString = "";

            //public string mode;              //制式：GSM,TD-SCDMA,WCDMA,LTE-TDD,LTE-FDD'
            //public string primaryplmn;       //主plmn
            //public string earfcndl;          //工作上行频点
            //public string earfcnul;          //工作下行频点
            //public string cellid;            //cellid, 2018-06-26
            //public string pci;               //工作pci
            //public string bandwidth;         //工作带宽
            //public string tac;               //TAC
            //public string txpower;           //功率衰减
            //public string periodtac;         //TAC变化周期
            //public string manualfreq;        //选频方式 0：自动选频 1：手动选频
            //public string bootMode;          //设备启动方式 0：半自动 1：全自动
            //public string Earfcnlist;        //频点列表，如：38950,39150
            //public string Bandoffset;        //频偏":"39,70000;38,10000
            //public string NTP;               //NTP服务器ip
            //public string ntppri;            //Ntp的优先级
            //public string source;            //同步源（0：GPS ； 1：CNM ； 2：no sync）
            //public string ManualEnable;      //是否设定手动同步源
            //public string ManualEarfcn;      //手动设置同步频点
            //public string ManualPci;         //手动设置同步pci
            //public string ManualBw;          //手动设置同步带宽
            //public string gpsConfig;         //GPS配置，0表示NOGPS，1表示GPS

            //public string otherplmn;         //多PLMN选项，多个之间用逗号隔开
            //public string periodFreq;        //{周期:freq1,freq2,freq3}

            genParaString += string.Format("[{0}]", apGP.mode);
            genParaString += string.Format("[{0}]", apGP.primaryplmn);
            genParaString += string.Format("[{0}]", apGP.earfcndl);
            genParaString += string.Format("[{0}]", apGP.earfcnul);
            genParaString += string.Format("[{0}]", apGP.cellid);
            genParaString += string.Format("[{0}]", apGP.pci);
            genParaString += string.Format("[{0}]", apGP.bandwidth);
            genParaString += string.Format("[{0}]", apGP.tac);
            genParaString += string.Format("[{0}]", apGP.txpower);
            genParaString += string.Format("[{0}]", apGP.periodtac);
            genParaString += string.Format("[{0}]", apGP.manualfreq);
            genParaString += string.Format("[{0}]", apGP.bootMode);
            genParaString += string.Format("[{0}]", apGP.Earfcnlist);
            genParaString += string.Format("[{0}]", apGP.Bandoffset);
            genParaString += string.Format("[{0}]", apGP.NTP);
            genParaString += string.Format("[{0}]", apGP.ntppri);
            genParaString += string.Format("[{0}]", apGP.source);
            genParaString += string.Format("[{0}]", apGP.ManualEnable);
            genParaString += string.Format("[{0}]", apGP.ManualEarfcn);
            genParaString += string.Format("[{0}]", apGP.ManualPci);
            genParaString += string.Format("[{0}]", apGP.ManualBw);
            genParaString += string.Format("[{0}]", apGP.gpsConfig);

            genParaString += string.Format("[{0}]", apGP.otherplmn);
            genParaString += string.Format("[{0}]", apGP.periodFreq);

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在AP通用参数表中删除指定的记录 
        /// </summary>  
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int ap_general_para_record_delete(int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == ap_general_para_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from ap_general_para where affDeviceId = {0}", affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 15-update_info操作

        /// <summary>
        /// 检查文件的MD5校验和记录是否存在
        /// </summary>
        /// <param name="md5sum"></param>  
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int update_info_record_exist(string md5sum)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(md5sum))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (md5sum.Length > 64 )
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from update_info where md5sum = '{0}' ", md5sum);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 检查文件的MD5校验和记录是否存在
        /// </summary>
        /// <param name="md5sum"></param>  
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int update_info_record_exist_filename(string fileName)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (fileName.Length > 255)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from update_info where fileName = '{0}' ", fileName);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到升级信息表中
        /// </summary>
        /// <param name="md5sum"></param>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   EXIST          ：记录已经存在
        ///   RC.SUCCESS     ：成功 
        /// </returns>
        public int update_info_record_insert(string md5sum, string fileName,string version)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(md5sum) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(version))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (md5sum.Length > 64 || fileName.Length > 255 || version.Length > 255)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查MD5校验和记录是否存在
            if ((int)RC.EXIST == update_info_record_exist(md5sum))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }                        

            string sql = string.Format("insert into update_info values(NULL,'{0}','{1}','{2}',now())", md5sum, fileName,version);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在升级信息表中删除指定的记录 
        /// </summary>
        /// <param name="md5sum"></param>
        /// <returns>
        ///   RC.NO_OPEN     ：数据库尚未打开
        ///   RC.PAR_NULL    ：参数为空
        ///   PAR_LEN_ERR    ：参数长度有误
        ///   RC.OP_FAIL     ：数据库操作失败 
        ///   NO_EXIST       ：记录不存在
        ///   RC.SUCCESS     ：成功
        /// </returns>
        public int update_info_record_delete(string md5sum)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(md5sum))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (md5sum.Length > 64)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == update_info_record_exist(md5sum))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from update_info where md5sum = '{0}'", md5sum);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取升级信息表中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,md5sum,fileName,version,time
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功 
        /// </returns>
        public int update_info_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("update_info");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.Int32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "md5sum";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "fileName";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "version";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.String");
            column4.ColumnName = "time";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);

            string sql = string.Format("select * from update_info");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row[0] = Convert.ToInt32(dr[0]);
                            row[1] = dr[1].ToString();
                            row[2] = dr[2].ToString();
                            row[3] = dr[3].ToString();
                            row[4] = dr[4].ToString();

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 16-device_unknown操作

        /// <summary>
        /// 检查设备(未指派)记录是否存在
        /// </summary>
        /// <param name="ipAddr">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.PAR_NULL ：参数为空
        ///   PAR_LEN_ERR ：参数长度有误
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int device_unknown_record_exist(string ipAddr, int port)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (port > 65535)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            string sql = string.Format("select count(*) from device_unknown where ipAddr = '{0}' and port = {1}", ipAddr, port);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到设备(未指派)表中       
        /// </summary>
        /// <param name="ipAddr">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.EXIST          ：记录已经存在
        ///   RC.IS_NOT_STATION ：域ID不是站点
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int device_unknown_record_insert(string ipAddr, int port)
        {       
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (port > 65535)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == device_unknown_record_exist(ipAddr,port))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string name = string.Format("{0}:{1}", ipAddr, port);
           
            string sql = string.Format("insert into device_unknown(id,name,ipAddr, port,lastOnline) values(NULL,'{0}','{1}',{2},now())", name,ipAddr, port);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;           
        }

        /// <summary>
        /// 通过IP地址和端口更新记录到设备(未指派)表中
        /// </summary>
        /// <param name="ipAddr">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="dev">
        /// 更新的字段如下，那些字段不为空就更新那些
        /// 
        /// sn;            //SN，GSM或第三方设备可能没有该字段
        /// ipAddr;        //IP地址
        /// port;          //端口号
        /// netmask;       //掩码
        /// mode;          //设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等
        /// online;        //上下线标识，0：下线；1：上线
        /// lastOnline;    //最后的上线时间
        /// isActive;      //标识该设备是否生效，0：无效；1：生效
        /// innerType;     //用于软件内部处理
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.MODIFIED_EXIST ：修改后的记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int device_unknown_record_update(string ipAddr, int port, strDevice dev)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (port > 65535)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }


            //检查记录是否存在
            if ((int)RC.NO_EXIST == device_unknown_record_exist(ipAddr,port))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }           

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(dev.name))
            {
                if (dev.name.Length > 64)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("name = '{0}',", dev.name);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(dev.sn))
            {
                if (dev.sn.Length > 32)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("sn = '{0}',", dev.sn);
                }
            }                        

            //(5)
            if (!string.IsNullOrEmpty(dev.netmask))
            {
                if (dev.netmask.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("netmask = '{0}',", dev.netmask);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(dev.mode))
            {
                if (dev.mode.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("mode = '{0}',", dev.mode);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(dev.online))
            {
                if ((dev.online == "0") || (dev.online == "1"))
                {
                    sqlSub += string.Format("online = {0},", dev.online);
                }
            }


            //(8)
            if (!string.IsNullOrEmpty(dev.lastOnline))
            {
                try
                {
                    DateTime.Parse(dev.lastOnline);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);
                    return (int)RC.TIME_FMT_ERR;
                }

                sqlSub += string.Format("lastOnline = '{0}',", dev.lastOnline);
            }

            //(9)
            if (!string.IsNullOrEmpty(dev.isActive))
            {
                if ((dev.isActive == "0") && (dev.isActive == "1"))
                {
                    sqlSub += string.Format("isActive = {0},", dev.isActive);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(dev.innerType))
            {
                if (dev.innerType.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("innerType = '{0}',", dev.innerType);
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update device_unknown set {0} where ipAddr = '{1}' and port = {2}", sqlSub, ipAddr, port);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在设备(未指派)表中删除指定的记录 
        /// </summary>  
        /// <param name="ipAddr">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int device_unknown_record_delete(string ipAddr, int port)
        {            
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (port > 65535)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == device_unknown_record_exist(ipAddr,port))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }         

            string sql = string.Format("delete from device_unknown where ipAddr = '{0}' and port = {1}", ipAddr, port);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在设备(未指派)表中清空所有的记录 
        /// </summary>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.PAR_NULL     ：参数为空
        ///   PAR_LEN_ERR     ：参数长度有误
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int device_unknown_record_clear()
        {           
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }
           
            string sql = string.Format("delete from device_unknown");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 获取设备表(未指派)中的各条记录
        /// </summary>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,sn,ipAddr
        /// port,netmask,mode,online,lastOnline,isActive,innerType,affDomainId
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_unknown_record_entity_get(ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            dt = new DataTable("device_unknown");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "sn";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "ipAddr";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.UInt16");
            column4.ColumnName = "port";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "netmask";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "mode";

            DataColumn column7 = new DataColumn();
            column7.DataType = System.Type.GetType("System.Int16");
            column7.ColumnName = "online";

            DataColumn column8 = new DataColumn();
            column8.DataType = System.Type.GetType("System.String");
            column8.ColumnName = "lastOnline";

            DataColumn column9 = new DataColumn();
            column9.DataType = System.Type.GetType("System.Int16");
            column9.ColumnName = "isActive";

            DataColumn column10 = new DataColumn();
            column10.DataType = System.Type.GetType("System.String");
            column10.ColumnName = "innerType";

            DataColumn column11 = new DataColumn();
            column11.DataType = System.Type.GetType("System.Int32");
            column11.ColumnName = "affDomainId";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);

            string sql = string.Format("select * from device_unknown");
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToUInt32(dr["id"]);
                            row["name"] = dr["name"].ToString();
                            row["sn"] = dr["sn"].ToString();                            

                            row["ipAddr"] = dr["ipAddr"].ToString();

                            if (dr["port"].ToString() != "")
                            {
                                row["port"] = Convert.ToUInt16(dr["port"]);
                            }
                            else
                            {
                                row["port"] = 0;
                            }

                            row["netmask"] = dr["netmask"].ToString();
                            row["mode"] = dr["mode"].ToString();

                            if (dr["online"].ToString() != "")
                            {
                                row["online"] = Convert.ToInt16(dr["online"]);
                            }
                            else
                            {
                                row["online"] = 0;
                            }

                            row["lastOnline"] = dr["lastOnline"].ToString();

                            if (dr["isActive"].ToString() != "")
                            {
                                row["isActive"] = Convert.ToInt16(dr["isActive"]);
                            }
                            else
                            {
                                row["isActive"] = 0;
                            }

                            row["innerType"] = dr["innerType"].ToString();

                            if (dr["affDomainId"].ToString() != "")
                            {
                                row["affDomainId"] = Convert.ToInt32(dr["affDomainId"]);
                            }
                            else
                            {
                                row["affDomainId"] = -1;
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过IP地址和端口获取设备表中的各条记录
        /// </summary>
        /// <param name="ipAddr">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="dt">
        /// 返回的DataTable，包含的列为：id,name,sn,ipAddr
        /// port,netmask,mode,online,lastOnline,isActive,innerType,affDomainId
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.PAR_NULL  ：参数为空
        ///   PAR_LEN_ERR  ：参数长度有误
        ///   RC.NO_OPEN   ：数据库尚未打开
        ///   RC.OP_FAIL   ：数据库操作失败 
        ///   RC.SUCCESS   ：成功
        /// </returns>
        public int device_unknown_record_entity_get_by_ipaddr_port(string ipAddr, int port, ref DataTable dt)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(ipAddr))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (ipAddr.Length > 32)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (port > 65535)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            dt = new DataTable("device_unknown");

            DataColumn column0 = new DataColumn();
            column0.DataType = System.Type.GetType("System.UInt32");
            column0.ColumnName = "id";

            DataColumn column1 = new DataColumn();
            column1.DataType = System.Type.GetType("System.String");
            column1.ColumnName = "name";

            DataColumn column2 = new DataColumn();
            column2.DataType = System.Type.GetType("System.String");
            column2.ColumnName = "sn";

            DataColumn column3 = new DataColumn();
            column3.DataType = System.Type.GetType("System.String");
            column3.ColumnName = "ipAddr";

            DataColumn column4 = new DataColumn();
            column4.DataType = System.Type.GetType("System.UInt16");
            column4.ColumnName = "port";

            DataColumn column5 = new DataColumn();
            column5.DataType = System.Type.GetType("System.String");
            column5.ColumnName = "netmask";

            DataColumn column6 = new DataColumn();
            column6.DataType = System.Type.GetType("System.String");
            column6.ColumnName = "mode";

            DataColumn column7 = new DataColumn();
            column7.DataType = System.Type.GetType("System.Int16");
            column7.ColumnName = "online";

            DataColumn column8 = new DataColumn();
            column8.DataType = System.Type.GetType("System.String");
            column8.ColumnName = "lastOnline";

            DataColumn column9 = new DataColumn();
            column9.DataType = System.Type.GetType("System.Int16");
            column9.ColumnName = "isActive";

            DataColumn column10 = new DataColumn();
            column10.DataType = System.Type.GetType("System.String");
            column10.ColumnName = "innerType";

            DataColumn column11 = new DataColumn();
            column11.DataType = System.Type.GetType("System.Int32");
            column11.ColumnName = "affDomainId";

            dt.Columns.Add(column0);
            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);

            string sql = string.Format("select * from device_unknown where ipAddr = '{0}' and port = {1}", ipAddr, port);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DataRow row = dt.NewRow();

                            row["id"] = Convert.ToUInt32(dr["id"]);
                            row["name"] = dr["name"].ToString();
                            row["sn"] = dr["sn"].ToString();                            

                            row["ipAddr"] = dr["ipAddr"].ToString();

                            if (dr["port"].ToString() != "")
                            {
                                row["port"] = Convert.ToUInt16(dr["port"]);
                            }
                            else
                            {
                                row["port"] = 0;
                            }

                            row["netmask"] = dr["netmask"].ToString();
                            row["mode"] = dr["mode"].ToString();

                            if (dr["online"].ToString() != "")
                            {
                                row["online"] = Convert.ToInt16(dr["online"]);
                            }
                            else
                            {
                                row["online"] = 0;
                            }

                            row["lastOnline"] = dr["lastOnline"].ToString();

                            if (dr["isActive"].ToString() != "")
                            {
                                row["isActive"] = Convert.ToInt16(dr["isActive"]);
                            }
                            else
                            {
                                row["isActive"] = 0;
                            }

                            row["innerType"] = dr["innerType"].ToString();

                            if (dr["affDomainId"].ToString() != "")
                            {
                                row["affDomainId"] = Convert.ToInt32(dr["affDomainId"]);
                            }
                            else
                            {
                                row["affDomainId"] = -1;
                            }

                            dt.Rows.Add(row);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 17-gsm_sys_para操作

        /// <summary>
        /// 检查gsm_sys_para记录是否存在
        /// </summary>
        /// <param name="carry">载波ID</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int gsm_sys_para_record_exist(int carry,int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from gsm_sys_para where carry = {0}  and affDeviceId = {1}", carry,affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gsm_sys_para表中
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>        
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_para_record_insert(int carry,int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gsm_sys_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            if (carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }

            string sql = string.Format("insert into gsm_sys_para(id,carry,bindingDevId,affDeviceId) values(NULL,{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gsm_sys_para表中
        /// </summary>
        /// <param name="carry">载波ID</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gsp">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_para_record_update(int carry, int affDeviceId, strGsmSysPara gsp)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(gsp.paraMcc))
            {
                if (gsp.paraMcc.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {                   
                    sqlSub += string.Format("paraMcc = {0},", gsp.paraMcc);                  
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gsp.paraMnc))
            {
                if (gsp.paraMnc.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraMnc = {0},", gsp.paraMnc);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gsp.paraBsic))
            {
                if (gsp.paraBsic.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraBsic = {0},", gsp.paraBsic);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gsp.paraLac))
            {
                if (gsp.paraLac.Length > 6)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraLac = {0},", gsp.paraLac);
                }
            }

            //(5) 
            if (!string.IsNullOrEmpty(gsp.paraCellId))
            {
                if (gsp.paraCellId.Length > 6)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraCellId = {0},", gsp.paraCellId);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(gsp.paraC2))
            {
                if (gsp.paraC2.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraC2 = {0},", gsp.paraC2);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(gsp.paraPeri))
            {
                if (gsp.paraPeri.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraPeri = {0},", gsp.paraPeri);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(gsp.paraAccPwr))
            {
                if (gsp.paraAccPwr.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraAccPwr = {0},", gsp.paraAccPwr);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(gsp.paraMsPwr))
            {
                if (gsp.paraMsPwr.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraMsPwr = {0},", gsp.paraMsPwr);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(gsp.paraRejCau))
            {
                if (gsp.paraRejCau.Length > 3)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("paraRejCau = {0},", gsp.paraRejCau);
                }
            }

            //(11)
            if (!string.IsNullOrEmpty(gsp.bindingDevId))
            {
                if (gsp.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", gsp.bindingDevId);
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gsm_sys_para set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry,affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gsm_sys_para记录
        /// </summary>
        /// <param name="carry">载波ID</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gsp">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_para_record_get_by_devid(int carry, int affDeviceId, ref strGsmSysPara gsp)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            gsp = new strGsmSysPara();
            string sql = string.Format("select * from gsm_sys_para where carry = {0} and affDeviceId = {1}", carry,affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["paraMcc"].ToString()))
                            {
                                gsp.paraMcc = dr["paraMcc"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraMnc"].ToString()))
                            {
                                gsp.paraMnc = dr["paraMnc"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraBsic"].ToString()))
                            {
                                gsp.paraBsic = dr["paraBsic"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraLac"].ToString()))
                            {
                                gsp.paraLac = dr["paraLac"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraCellId"].ToString()))
                            {
                                gsp.paraCellId = dr["paraCellId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraC2"].ToString()))
                            {
                                gsp.paraC2 = dr["paraC2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraPeri"].ToString()))
                            {
                                gsp.paraPeri = dr["paraPeri"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraAccPwr"].ToString()))
                            {
                                gsp.paraAccPwr = dr["paraAccPwr"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraMsPwr"].ToString()))
                            {
                                gsp.paraMsPwr = dr["paraMsPwr"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["paraRejCau"].ToString()))
                            {
                                gsp.paraRejCau = dr["paraRejCau"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gsp.bindingDevId = dr["bindingDevId"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gsm_sys_para表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波ID</param>   
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gsm_sys_para_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gsm_sys_para where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 18-gsm_sys_option操作

        /// <summary>
        /// 检查gsm_sys_option记录是否存在
        /// </summary>
        /// <param name="carry">载波ID</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int gsm_sys_option_record_exist(int carry,int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from gsm_sys_option where carry = {0} and affDeviceId = {1}", carry,affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gsm_sys_option表中
        /// </summary>
        /// <param name="carry">载波ID</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_option_record_insert(int carry,int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gsm_sys_option_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into gsm_sys_option(id,carry,bindingDevId,affDeviceId) values(NULL,{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gsm_sys_option表中
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gso">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_option_record_update(int carry,int affDeviceId, strGsmSysOption gso)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_option_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(gso.opLuSms))
            {
                if (gso.opLuSms.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opLuSms = {0},", gso.opLuSms);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gso.opLuImei))
            {
                if (gso.opLuImei.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opLuImei = {0},", gso.opLuImei);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gso.opCallEn))
            {
                if (gso.opCallEn.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opCallEn = {0},", gso.opCallEn);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gso.opDebug))
            {
                if (gso.opDebug.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opDebug = {0},", gso.opDebug);
                }
            }

            //(5) 
            if (!string.IsNullOrEmpty(gso.opLuType))
            {
                if (gso.opLuType.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opLuType = {0},", gso.opLuType);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(gso.opSmsType))
            {
                if (gso.opSmsType.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opSmsType = {0},", gso.opSmsType);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(gso.opRegModel))
            {
                if (gso.opRegModel.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("opRegModel = {0},", gso.opRegModel);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(gso.bindingDevId))
            {
                if (gso.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", gso.bindingDevId);
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gsm_sys_option set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry,affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gsm_sys_option记录
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gso">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_sys_option_record_get_by_devid(int carry,int affDeviceId, ref strGsmSysOption gso)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_option_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            gso = new strGsmSysOption();
            string sql = string.Format("select * from gsm_sys_option where carry = {0} and affDeviceId = {1}", carry,affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["opLuSms"].ToString()))
                            {
                                gso.opLuSms = dr["opLuSms"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opLuImei"].ToString()))
                            {
                                gso.opLuImei = dr["opLuImei"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opCallEn"].ToString()))
                            {
                                gso.opCallEn = dr["opCallEn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opDebug"].ToString()))
                            {
                                gso.opDebug = dr["opDebug"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opLuType"].ToString()))
                            {
                                gso.opLuType = dr["opLuType"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opSmsType"].ToString()))
                            {
                                gso.opSmsType = dr["opSmsType"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["opRegModel"].ToString()))
                            {
                                gso.opRegModel = dr["opRegModel"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gso.bindingDevId = dr["bindingDevId"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gsm_sys_para表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gsm_sys_option_record_delete(int carry,int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_sys_option_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gsm_sys_option where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 19-gsm_rf_para操作

        /// <summary>
        /// 检查gsm_rf_para记录是否存在
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int gsm_rf_para_record_exist(int carry,int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from gsm_rf_para where carry = {0} and affDeviceId = {1}", carry,affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gsm_rf_para表中
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_rf_para_record_insert(int carry,int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gsm_rf_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into gsm_rf_para(id,carry,bindingDevId,affDeviceId) values(NULL,{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gsm_rf_para表中
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="grp">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_rf_para_record_update(int carry,int affDeviceId, strGsmRfPara grp)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_rf_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(grp.rfEnable))
            {
                if (grp.rfEnable.Length > 1)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("rfEnable = {0},", grp.rfEnable);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(grp.rfFreq))
            {
                if (grp.rfFreq.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("rfFreq = {0},", grp.rfFreq);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(grp.rfPwr))
            {
                if (grp.rfPwr.Length > 2)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("rfPwr = {0},", grp.rfPwr);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(grp.bindingDevId))
            {
                if (grp.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", grp.bindingDevId);
                }
            }


            // 2018-07-23
            if (!string.IsNullOrEmpty(grp.res1))
            {
                if (grp.res1.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res1 = '{0}',", grp.res1);
                }
            }

            if (!string.IsNullOrEmpty(grp.res2))
            {
                if (grp.res2.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res2 = '{0}',", grp.res2);
                }
            }

            if (!string.IsNullOrEmpty(grp.res3))
            {
                if (grp.res3.Length > 128)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res3 = '{0}',", grp.res3);
                }
            }


            //(5,6)
            if (!string.IsNullOrEmpty(grp.activeTime1Start) && !string.IsNullOrEmpty(grp.activeTime1Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(grp.activeTime1Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(grp.activeTime1Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime1Start = '{0}',activeTime1Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(7,8)
            if (!string.IsNullOrEmpty(grp.activeTime2Start) && !string.IsNullOrEmpty(grp.activeTime2Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(grp.activeTime2Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(grp.activeTime2Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime2Start = '{0}',activeTime2Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(9,10)
            if (!string.IsNullOrEmpty(grp.activeTime3Start) && !string.IsNullOrEmpty(grp.activeTime3Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(grp.activeTime3Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(grp.activeTime3Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime3Start = '{0}',activeTime3Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(11,12)
            if (!string.IsNullOrEmpty(grp.activeTime4Start) && !string.IsNullOrEmpty(grp.activeTime4Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(grp.activeTime4Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(grp.activeTime4Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime4Start = '{0}',activeTime4Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gsm_rf_para set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry,affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gsm_sys_para记录
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="grp">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_rf_para_record_get_by_devid(int carry,int affDeviceId, ref strGsmRfPara grp)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_rf_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            grp = new strGsmRfPara();
            string sql = string.Format("select * from gsm_rf_para where carry = {0} and affDeviceId = {1}",carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["rfEnable"].ToString()))
                            {
                                grp.rfEnable = dr["rfEnable"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["rfFreq"].ToString()))
                            {
                                grp.rfFreq = dr["rfFreq"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["rfPwr"].ToString()))
                            {
                                grp.rfPwr = dr["rfPwr"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                grp.bindingDevId = dr["bindingDevId"].ToString();
                            }

                            // 2018-07-23
                            if (!string.IsNullOrEmpty(dr["res1"].ToString()))
                            {
                                grp.res1 = dr["res1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res2"].ToString()))
                            {
                                grp.res2 = dr["res2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res3"].ToString()))
                            {
                                grp.res3 = dr["res3"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime1Start"].ToString()))
                            {
                                grp.activeTime1Start = dr["activeTime1Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime1Ended"].ToString()))
                            {
                                grp.activeTime1Ended = dr["activeTime1Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime2Start"].ToString()))
                            {
                                grp.activeTime2Start = dr["activeTime2Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime2Ended"].ToString()))
                            {
                                grp.activeTime2Ended = dr["activeTime2Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime3Start"].ToString()))
                            {
                                grp.activeTime3Start = dr["activeTime3Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime3Ended"].ToString()))
                            {
                                grp.activeTime3Ended = dr["activeTime3Ended"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime4Start"].ToString()))
                            {
                                grp.activeTime4Start = dr["activeTime4Start"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["activeTime4Ended"].ToString()))
                            {
                                grp.activeTime4Ended = dr["activeTime4Ended"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gsm_rf_para表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gsm_rf_para_record_delete(int carry,int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gsm_rf_para_record_exist(carry,affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gsm_rf_para where carry = {0} and affDeviceId = {1}", carry,affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号和载波id获取GSM的所有参数string
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <param name="gsmAllParaString">所有参数string</param>    
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_all_record_get_by_devid(int carry, int affDeviceId, ref string gsmAllParaString)
        {
            int rtv = -1;    
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            // RECV_SYS_PARA
            // public string paraMcc;      //移动国家码
            // public string paraMnc;      //移动网号
            // public string paraBsic;     //基站识别码
            // public string paraLac;      //位置区号
            // public string paraCellId;   //小区ID
            // public string paraC2;       //C2偏移量
            // public string paraPeri;     //周期性位置更新周期
            // public string paraAccPwr;   //接入功率
            // public string paraMsPwr;    //手机发射功率
            // public string paraRejCau;   //位置更新拒绝原因

            // RECV_SYS_OPTION
            // public string opLuSms;      //登录时发送短信
            // public string opLuImei;     //登录时获取IMEI
            // public string opCallEn;     //允许用户主叫
            // public string opDebug;      //调试模式，上报信令
            // public string opLuType;     //登录类型
            // public string opSmsType;    //短信类型
            // public string opRegModel;   //注册工作模式  【该字段在RECV_REG_MODE中获取到的】

            // RECV_RF_PARA
            // public string rfEnable;          //射频使能
            // public string rfFreq;            //信道号
            // public string rfPwr;             //发射功率衰减值    

            // RECV_SMS_OPTION  //短信部分无需处理

            // RECV_REG_MODE

            strGsmSysPara gsp = new strGsmSysPara();
            rtv = gsm_sys_para_record_get_by_devid(carry, affDeviceId, ref gsp);
            if ((int)RC.SUCCESS != rtv)
            {
                return rtv;
            }

            gsmAllParaString = "";
            gsmAllParaString += string.Format("[{0}]", gsp.paraMcc);
            gsmAllParaString += string.Format("[{0}]", gsp.paraMnc);
            gsmAllParaString += string.Format("[{0}]", gsp.paraBsic);
            gsmAllParaString += string.Format("[{0}]", gsp.paraLac);
            gsmAllParaString += string.Format("[{0}]", gsp.paraCellId);
            gsmAllParaString += string.Format("[{0}]", gsp.paraC2);
            gsmAllParaString += string.Format("[{0}]", gsp.paraPeri);
            gsmAllParaString += string.Format("[{0}]", gsp.paraAccPwr);
            gsmAllParaString += string.Format("[{0}]", gsp.paraMsPwr);
            gsmAllParaString += string.Format("[{0}]", gsp.paraRejCau);

            strGsmSysOption gso = new strGsmSysOption();
            rtv = gsm_sys_option_record_get_by_devid(carry, affDeviceId, ref gso);
            if ((int)RC.SUCCESS != rtv)
            {
                return rtv;
            }
           
            gsmAllParaString += string.Format("[{0}]", gso.opLuSms);
            gsmAllParaString += string.Format("[{0}]", gso.opLuImei);
            gsmAllParaString += string.Format("[{0}]", gso.opCallEn);
            gsmAllParaString += string.Format("[{0}]", gso.opDebug);
            gsmAllParaString += string.Format("[{0}]", gso.opLuType);
            gsmAllParaString += string.Format("[{0}]", gso.opSmsType);

            strGsmRfPara grp = new strGsmRfPara();
            rtv = gsm_rf_para_record_get_by_devid(carry, affDeviceId, ref grp);
            if ((int)RC.SUCCESS != rtv)
            {
                return rtv;
            }


            gsmAllParaString += string.Format("[{0}]", grp.rfEnable);
            gsmAllParaString += string.Format("[{0}]", grp.rfFreq);
            gsmAllParaString += string.Format("[{0}]", grp.rfPwr);

            //RECV_REG_MODE
            gsmAllParaString += string.Format("[{0}]", gso.opRegModel);

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 20-gsm_msg_option操作

        /// <summary>
        /// 插入记录到gsm_msg_option表中
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_msg_option_insert(int carry, int affDeviceId, strGsmMsgOption gmo)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("insert into gsm_msg_option values(NULL,'{0}','{1}','{2}','{3}',{4},{5},{6},{7},{8},-1,{9})",
               gmo.smsRPOA, 
               gmo.smsTPOA, 
               gmo.smsSCTS, 
               gmo.smsDATA,
               int.Parse(gmo.autoSend),
               int.Parse(gmo.autoFilterSMS),
               int.Parse(gmo.delayTime),
               int.Parse(gmo.smsCoding), 
               carry, 
               affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gsm_msg_option记录
        /// </summary>
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="listGMO">返回的记录列表</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gsm_msg_option_get_by_devid(int carry, int affDeviceId, ref List<strGsmMsgOption> listGMO)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            listGMO = new List<strGsmMsgOption>();
            
            string sql = string.Format("select * from gsm_msg_option where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            strGsmMsgOption gmo = new strGsmMsgOption();

                            if (!string.IsNullOrEmpty(dr["smsRPOA"].ToString()))
                            {
                                gmo.smsRPOA = dr["smsRPOA"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["smsTPOA"].ToString()))
                            {
                                gmo.smsTPOA = dr["smsTPOA"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["smsSCTS"].ToString()))
                            {
                                gmo.smsSCTS = dr["smsSCTS"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["smsDATA"].ToString()))
                            {
                                gmo.smsDATA = dr["smsDATA"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["autoSend"].ToString()))
                            {
                                gmo.autoSend = dr["autoSend"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["autoFilterSMS"].ToString()))
                            {
                                gmo.autoFilterSMS = dr["autoFilterSMS"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["delayTime"].ToString()))
                            {
                                gmo.delayTime = dr["delayTime"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["smsCoding"].ToString()))
                            {
                                gmo.smsCoding = dr["smsCoding"].ToString();
                            }                            

                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gmo.bindingDevId = dr["bindingDevId"].ToString();
                            }

                            listGMO.Add(gmo);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gsm_msg_option表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波标识</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gsm_msg_option_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("delete from  gsm_msg_option where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 21-redirection操作

        /// <summary>
        /// 检查redirection记录是否存在
        /// </summary>
        /// <param name="category">
        /// 名单类别 
        /// 0:white,1:black,2:other
        /// </param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN  ：数据库尚未打开
        ///   RC.OP_FAIL  ：数据库操作失败 
        ///   RC.NO_EXIST ：不存在
        ///   RC.EXIST    ：存在
        /// </returns>
        public int redirection_record_exist(int category, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("select count(*) from redirection where category = {0} and affDeviceId = {1}", category, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gsm_rf_para表中
        /// </summary>
        /// <param name="category">
        /// 名单类别 
        /// 0:white,1:black,2:other
        /// </param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int redirection_record_insert(int category, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            if (category != 0 && category != 1 && category != 2)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == redirection_record_exist(category, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into redirection(category,affDeviceId) values({0},{1})", category, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 检查uarfcn集合的合法性，返回过滤后的字符串（最多7个）
        /// </summary>
        /// <param name="uarfcnSet">集合以逗号分隔，如下1,2,3,4,5,6,7,8,9</param>
        /// <param name="checkOk">检查后的结果1,2,3,4,5,6,7</param>
        /// <returns>
        /// true  ： 合法
        /// false ： 非法
        /// </returns>
        private bool check_and_get_uarfcn_list_set(string uarfcnSet, ref string checkOk)
        {
            if (string.IsNullOrEmpty(uarfcnSet))
            {
                Logger.Trace(LogInfoType.EROR, "uarfcnSet参数为空", "DB", LogCategory.I);
                return false;
            }

            if (uarfcnSet.Length > 1024)
            {
                Logger.Trace(LogInfoType.EROR, "uarfcnSet参数长度有误", "DB", LogCategory.I);
                return false;
            }

            List<string> listStr = new List<string>();
            string[] s = uarfcnSet.Split(new char[] { ',' });

            if (s.Length <= 0)
            {
                return false;
            }
            else
            {
                foreach (string str in s)
                {
                    try
                    {
                        UInt16.Parse(str);
                        listStr.Add(str);
                    }
                    catch (Exception ee)
                    {
                        Logger.Trace(LogInfoType.EROR, ee.Message, "DB", LogCategory.I);                  
                    }
                }
            }

            if (listStr.Count > 0)
            {
                int cnt = -1;
                checkOk = "";

                if (listStr.Count > 7)
                {
                    cnt = 7;
                }
                else
                {
                    cnt = listStr.Count;
                }

                for (int i = 0; i < cnt; i++)
                {
                    if (i == (cnt - 1))
                    {
                        checkOk += listStr[i];
                    }
                    else
                    {
                        checkOk += listStr[i] + ",";
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 更新记录到gsm_rf_para表中
        /// </summary>
        /// <param name="category">
        /// 名单类别 
        /// 0:white,1:black,2:other
        /// </param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="rd">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int redirection_record_update(int category, int affDeviceId, strRedirection rd)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (category != 0 && category != 1 && category != 2)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_FMT_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == redirection_record_exist(category, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            //public string priority;         //2:2G,3:3G,4:4G,Others:noredirect
            //public string GeranRedirect;    //0:disable;1:enable
            //public string arfcn;            //2G frequency    
            //public string UtranRedirect;    //0:disable;1:enable
            //public string uarfcn;           //3G frequency
            //public string EutranRedirect;   //0:disable;1:enable
            //public string earfcn;           //4G frequency
            //public string RejectMethod;     //1,2,0xFF,0x10-0xFE
            //public string additionalFreq;   //uarfcn,uarfcn;不超过7个freq，超过7个freq的默认丢弃

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(rd.priority))
            {
                if (rd.priority.Length > 1)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("priority = {0},", rd.priority);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(rd.GeranRedirect))
            {
                if (rd.GeranRedirect.Length > 1)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("GeranRedirect = {0},", rd.GeranRedirect);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(rd.arfcn))
            {
                if (rd.arfcn.Length > 6)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("arfcn = {0},", rd.arfcn);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(rd.UtranRedirect))
            {
                if (rd.UtranRedirect.Length > 1)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("UtranRedirect = {0},", rd.UtranRedirect);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(rd.uarfcn))
            {
                if (rd.uarfcn.Length > 6)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("uarfcn = {0},", rd.uarfcn);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(rd.EutranRedirect))
            {
                if (rd.EutranRedirect.Length > 1)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("EutranRedirect = {0},", rd.EutranRedirect);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(rd.earfcn))
            {
                if (rd.earfcn.Length > 6)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("earfcn = {0},", rd.earfcn);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(rd.RejectMethod))
            {
                if (rd.RejectMethod.Length > 4)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("RejectMethod = {0},", rd.RejectMethod);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(rd.additionalFreq))
            {
                string checkOk = "";
                if (check_and_get_uarfcn_list_set(rd.additionalFreq, ref checkOk) == true)
                {
                    sqlSub += string.Format("additionalFreq = '{0}',", checkOk);
                }
                else
                {
                    Logger.Trace(LogInfoType.EROR, "additionalFreq解析出错.", "DB", LogCategory.I);
                }               
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update redirection set {0} where category = {1} and affDeviceId = {2}", sqlSub, category, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取redirection记录
        /// </summary>
        /// <param name="category">
        /// 名单类别 
        /// 0:white,1:black,2:other
        /// </param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="rd">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int redirection_record_get_by_devid(int category, int affDeviceId, ref strRedirection rd)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == redirection_record_exist(category, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            rd = new strRedirection();
            string sql = string.Format("select * from redirection where category = {0} and affDeviceId = {1}", category, affDeviceId);

            //public string priority;         //2:2G,3:3G,4:4G,Others:noredirect
            //public string GeranRedirect;    //0:disable;1:enable
            //public string arfcn;            //2G frequency    
            //public string UtranRedirect;    //0:disable;1:enable
            //public string uarfcn;           //3G frequency
            //public string EutranRedirect;   //0:disable;1:enable
            //public string earfcn;           //4G frequency
            //public string RejectMethod;     //1,2,0xFF,0x10-0xFE
            //public string additionalFreq;   //uarfcn,uarfcn;不超过7个freq，超过7个freq的默认丢弃

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["priority"].ToString()))
                            {
                                rd.priority = dr["priority"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["GeranRedirect"].ToString()))
                            {
                                rd.GeranRedirect = dr["GeranRedirect"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["arfcn"].ToString()))
                            {
                                rd.arfcn = dr["arfcn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["UtranRedirect"].ToString()))
                            {
                                rd.UtranRedirect = dr["UtranRedirect"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["uarfcn"].ToString()))
                            {
                                rd.uarfcn = dr["uarfcn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["EutranRedirect"].ToString()))
                            {
                                rd.EutranRedirect = dr["EutranRedirect"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["earfcn"].ToString()))
                            {
                                rd.earfcn = dr["earfcn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["RejectMethod"].ToString()))
                            {
                                rd.RejectMethod = dr["RejectMethod"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["additionalFreq"].ToString()))
                            {
                                rd.additionalFreq = dr["additionalFreq"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gsm_rf_para表中删除指定的记录 
        /// </summary>  
        /// <param name="category">
        /// 名单类别 
        /// 0:white,1:black,2:other
        /// </param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int redirection_record_delete(int category, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == redirection_record_exist(category, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from redirection where category = {0} and affDeviceId = {1}", category, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 22-gc_nb_cell操作

        /// <summary>
        /// 从listItem中获取对应的字符串
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="rtvString"></param>
        /// <returns>0成功，-1失败</returns>
        private int get_nb_cell_item_2_str(List<strGcNbCellItem> listItem, ref string rtvString)
        {
            if (listItem == null || listItem.Count == 0)
            {
                return -1;
            }

            string sub = "";
            string big = "";

            //public string wUarfcn;      //小区频点
            //public string wPhyCellId;   //物理小区ID
            //public string cRSRP;        //信号功率
            //public string bReserved;    //保留
            //public string cC1;          //C1测量值
            //public string bC2;          //C2测量值

            foreach (strGcNbCellItem str in listItem)
            {
                sub = "";
                if (!string.IsNullOrEmpty(str.wUarfcn))
                {
                    sub += string.Format("{0}:", str.wUarfcn);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (!string.IsNullOrEmpty(str.wPhyCellId))
                {
                    sub += string.Format("{0}:", str.wPhyCellId);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (!string.IsNullOrEmpty(str.cRSRP))
                {
                    sub += string.Format("{0}:", str.cRSRP);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (!string.IsNullOrEmpty(str.bReserved))
                {
                    sub += string.Format("{0}:", str.bReserved);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (!string.IsNullOrEmpty(str.cC1))
                {
                    sub += string.Format("{0}:", str.cC1);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (!string.IsNullOrEmpty(str.bC2))
                {
                    sub += string.Format("{0}:", str.bC2);
                }
                else
                {
                    sub += string.Format(":");
                }

                if (sub != "")
                {
                    //去掉最后一个字符
                    sub = sub.Remove(sub.Length - 1, 1);
                    sub = string.Format("({0}),", sub);

                    big += sub;
                }
            }

            if (big != "")
            {
                //去掉最后一个字符
                big = big.Remove(big.Length - 1, 1);
                rtvString = big;
            }

            return 0;
        }

        /// <summary>
        /// 从字符串获取对应的listItem
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="rtvString"></param>
        /// <returns>0成功，-1失败</returns>
        private int get_nb_cell_str_2_item(string strOri, ref List<strGcNbCellItem> listItem)
        {
            if (string.IsNullOrEmpty(strOri))
            {
                return -1;
            }

            string[] s = strOri.Split(new char[] { ',' });

            if (s.Length <= 0)
            {
                return -1;
            }
            else
            {
                string tmp = "";
                listItem = new List<strGcNbCellItem>();

                foreach (string str in s)
                {
                    if (!str.Contains("(") || !str.Contains(")"))
                    {
                        continue;
                    }

                    tmp = str;

                    //去掉首位的"()"
                    tmp = tmp.Remove(0, 1);
                    tmp = tmp.Remove(tmp.Length - 1, 1);

                    string[] strSub = tmp.Split(new char[] { ':' });

                    if (strSub.Length != 6)
                    {
                        continue;
                    }

                    strGcNbCellItem item = new strGcNbCellItem();
                    item.wUarfcn = strSub[0];
                    item.wPhyCellId = strSub[1];
                    item.cRSRP = strSub[2];
                    item.bReserved = strSub[3];
                    item.cC1 = strSub[4];
                    item.bC2 = strSub[5];

                    listItem.Add(item);
                }
            }

            return 0;
        }

        /// <summary>
        /// 在gc_nb_cell表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.CARRY_ERR    ：载波非法
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_nb_cell_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }           

            string sql = string.Format("delete from gc_nb_cell where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }        

        /// <summary>
        /// 批量插入记录到gc_nb_cell表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>        
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.CARRY_ERR      ：载波非法
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_nb_cell_record_insert_batch(int carry, int affDeviceId,List<strGcNbCell> list)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (list.Count <= 0)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {

                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            string sqlSub = "";
            string sqlBig = "";

            foreach (strGcNbCell str in list)
            {
                //(1)-id
                sqlSub = "NULL,";

                //(2)-bGCId
                if (!string.IsNullOrEmpty(str.bGCId))
                {
                    if (str.bGCId.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.bGCId);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(3)-bPLMNId
                if (!string.IsNullOrEmpty(str.bPLMNId))
                {
                    if (str.bPLMNId.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.bPLMNId);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }


                //(4)-cRSRP
                if (!string.IsNullOrEmpty(str.cRSRP))
                {
                    if (str.cRSRP.Length > 8)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.cRSRP);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(5)-wTac
                if (!string.IsNullOrEmpty(str.wTac))
                {
                    if (str.wTac.Length > 8)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.wTac);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(6)-wPhyCellId
                if (!string.IsNullOrEmpty(str.wPhyCellId))
                {
                    if (str.wPhyCellId.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.wPhyCellId);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(7)-wUARFCN
                if (!string.IsNullOrEmpty(str.wUARFCN))
                {
                    if (str.wUARFCN.Length > 8)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.wUARFCN);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(8)-cRefTxPower
                if (!string.IsNullOrEmpty(str.cRefTxPower))
                {
                    if (str.cRefTxPower.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.cRefTxPower);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(9)-bNbCellNum
                if (!string.IsNullOrEmpty(str.bNbCellNum))
                {
                    if (str.bNbCellNum.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.bNbCellNum);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(10)-bC2
                if (!string.IsNullOrEmpty(str.bC2))
                {
                    if (str.bC2.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.bC2);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(11)-bReserved1
                if (!string.IsNullOrEmpty(str.bReserved1))
                {
                    if (str.bReserved1.Length > 16)
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                        continue;
                    }
                    else
                    {
                        sqlSub += string.Format("'{0}',", str.bReserved1);
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }

                //(12)-nc_item
                if (str.listItem.Count > 0)
                {
                    string itemStr = "";
                    if (0 == get_nb_cell_item_2_str(str.listItem, ref itemStr))
                    {
                        sqlSub += string.Format("'{0}',", itemStr);
                    }
                    else
                    {
                        sqlSub += string.Format("NULL,");
                    }
                }
                else
                {
                    sqlSub += string.Format("NULL,");
                }                

                //(13,14,15)-carry,bindingDevId,affDeviceId
                sqlSub += string.Format("{0},NULL,{1},",carry,affDeviceId);

                if (sqlSub != "")
                {
                    //去掉最后一个字符
                    sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
                    sqlSub = string.Format("({0}),", sqlSub);

                    sqlBig += sqlSub;
                }
            }

            if (sqlBig != "")
            {
                //去掉最后一个字符
                sqlBig = sqlBig.Remove(sqlBig.Length - 1, 1);
            }

            string sql = string.Format("insert into gc_nb_cell values{0}", sqlBig);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gsm_sys_para记录
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="list">对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.CARRY_ERR      ：载波非法
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_nb_cell_record_get_by_devid(int carry, int affDeviceId, ref List<strGcNbCell> list)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            list = new List<strGcNbCell>();
            string sql = string.Format("select * from gc_nb_cell where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        strGcNbCell str = new strGcNbCell();
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["bGCId"].ToString()))
                            {
                                str.bGCId = dr["bGCId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bPLMNId"].ToString()))
                            {
                                str.bPLMNId = dr["bPLMNId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["cRSRP"].ToString()))
                            {
                                str.cRSRP = dr["cRSRP"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wTac"].ToString()))
                            {
                                str.wTac = dr["wTac"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wPhyCellId"].ToString()))
                            {
                                str.wPhyCellId = dr["wPhyCellId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wUARFCN"].ToString()))
                            {
                                str.wUARFCN = dr["wUARFCN"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["cRefTxPower"].ToString()))
                            {
                                str.cRefTxPower = dr["cRefTxPower"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bNbCellNum"].ToString()))
                            {
                                str.bNbCellNum = dr["bNbCellNum"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bC2"].ToString()))
                            {
                                str.bC2 = dr["bC2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bReserved1"].ToString()))
                            {
                                str.bReserved1 = dr["bReserved1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["nc_item"].ToString()))
                            {
                                str.nc_item = dr["nc_item"].ToString();
                                get_nb_cell_str_2_item(str.nc_item, ref str.listItem);
                            }

                            list.Add(str);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 23-gc_param_config操作

        /// <summary>
        /// 检查gc_param_config记录是否存在
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.CARRY_ERR    ：载波非法
        ///   RC.NO_EXIST     ：不存在
        ///   RC.EXIST        ：存在
        /// </returns>
        public int gc_param_config_record_exist(int carry, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("select count(*) from gc_param_config where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gc_param_config表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_param_config_record_insert(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gc_param_config_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into gc_param_config(id,carry,bindingDevId,affDeviceId) values(NULL,{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gc_param_config表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gpr">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_param_config_record_update(int carry, int affDeviceId, strGcParamConfig gpc)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_param_config_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(gpc.bWorkingMode))
            {
                if (gpc.bWorkingMode.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bWorkingMode = '{0}',", gpc.bWorkingMode);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gpc.bC))
            {
                if (gpc.bC.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bC = '{0}',", gpc.bC);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gpc.wRedirectCellUarfcn))
            {
                if (gpc.wRedirectCellUarfcn.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wRedirectCellUarfcn = '{0}',", gpc.wRedirectCellUarfcn);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gpc.dwDateTime))
            {
                if (gpc.dwDateTime.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("dwDateTime = '{0}',", gpc.dwDateTime);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(gpc.bPLMNId))
            {
                if (gpc.bPLMNId.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bPLMNId = '{0}',", gpc.bPLMNId);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(gpc.bTxPower))
            {
                if (gpc.bTxPower.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bTxPower = '{0}',", gpc.bTxPower);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(gpc.bRxGain))
            {
                if (gpc.bRxGain.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bRxGain = '{0}',", gpc.bRxGain);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(gpc.wPhyCellId))
            {
                if (gpc.wPhyCellId.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wPhyCellId = '{0}',", gpc.wPhyCellId);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(gpc.wLAC))
            {
                if (gpc.wLAC.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wLAC = '{0}',", gpc.wLAC);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(gpc.wUARFCN))
            {
                if (gpc.wUARFCN.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wUARFCN = '{0}',", gpc.wUARFCN);
                }
            }

            //(11)
            if (!string.IsNullOrEmpty(gpc.dwCellId))
            {
                if (gpc.dwCellId.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("dwCellId = '{0}',", gpc.dwCellId);
                }
            }
           

            //(12)
            if (!string.IsNullOrEmpty(gpc.res1))
            {
                if (gpc.res1.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res1 = '{0}',", gpc.res1);
                }
            }

            //(13)
            if (!string.IsNullOrEmpty(gpc.res2))
            {
                if (gpc.res2.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res2 = '{0}',", gpc.res2);
                }
            }

            //(14)
            if (!string.IsNullOrEmpty(gpc.res3))
            {
                if (gpc.res3.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("res3 = '{0}',", gpc.res3);
                }
            }

            //(15)
            if (!string.IsNullOrEmpty(gpc.bindingDevId))
            {
                if (gpc.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", gpc.bindingDevId);
                }
            }           

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gc_param_config set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gc_param_config记录
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gpr">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_param_config_record_get_by_devid(int carry, int affDeviceId, ref strGcParamConfig gpc)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_param_config_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            gpc = new strGcParamConfig();
            string sql = string.Format("select * from gc_param_config where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (!string.IsNullOrEmpty(dr["bWorkingMode"].ToString()))
                            {
                                gpc.bWorkingMode = dr["bWorkingMode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bC"].ToString()))
                            {
                                gpc.bC = dr["bC"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wRedirectCellUarfcn"].ToString()))
                            {
                                gpc.wRedirectCellUarfcn = dr["wRedirectCellUarfcn"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["dwDateTime"].ToString()))
                            {
                                gpc.dwDateTime = dr["dwDateTime"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bPLMNId"].ToString()))
                            {
                                gpc.bPLMNId = dr["bPLMNId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bTxPower"].ToString()))
                            {
                                gpc.bTxPower = dr["bTxPower"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bRxGain"].ToString()))
                            {
                                gpc.bRxGain = dr["bRxGain"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wPhyCellId"].ToString()))
                            {
                                gpc.wPhyCellId = dr["wPhyCellId"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wLAC"].ToString()))
                            {
                                gpc.wLAC = dr["wLAC"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wUARFCN"].ToString()))
                            {
                                gpc.wUARFCN = dr["wUARFCN"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["dwCellId"].ToString()))
                            {
                                gpc.dwCellId = dr["dwCellId"].ToString();
                            }


                            if (!string.IsNullOrEmpty(dr["res1"].ToString()))
                            {
                                gpc.res1 = dr["res1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res2"].ToString()))
                            {
                                gpc.res2 = dr["res2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res3"].ToString()))
                            {
                                gpc.res3 = dr["res3"].ToString();
                            }


                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gpc.bindingDevId = dr["bindingDevId"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gc_param_config表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_param_config_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_param_config_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gc_param_config where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion               

        #region 24-gc_misc操作

        /// <summary>
        /// 检查gc_misc记录是否存在
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.CARRY_ERR    ：载波非法
        ///   RC.NO_EXIST     ：不存在
        ///   RC.EXIST        ：存在
        /// </returns>
        public int gc_misc_record_exist(int carry, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("select count(*) from gc_misc where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gc_misc表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_misc_record_insert(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gc_misc_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }
          
            string sql = string.Format("insert into gc_misc(id,time,carry,bindingDevId,affDeviceId) values(NULL,now(),{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gc_misc表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gpr">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_misc_record_update(int carry, int affDeviceId, strGcMisc gm)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_misc_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(gm.wTraceLen))
            {
                if (gm.wTraceLen.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wTraceLen = '{0}',", gm.wTraceLen);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gm.cTrace))
            {
                if (gm.cTrace.Length > 1024)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("cTrace = '{0}',", gm.cTrace);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gm.bOrmType))
            {
                if (gm.bOrmType.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bOrmType = '{0}',", gm.bOrmType);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gm.bUeId))
            {
                if (gm.bUeId.Length > 16)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bUeId = '{0}',", gm.bUeId);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(gm.cRSRP))
            {
                if (gm.cRSRP.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("cRSRP = '{0}',", gm.cRSRP);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(gm.bUeContentLen))
            {
                if (gm.bUeContentLen.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bUeContentLen = '{0}',", gm.bUeContentLen);
                }
            }


            //(7)
            if (!string.IsNullOrEmpty(gm.bUeContent))
            {
                if (gm.bUeContent.Length > 512)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bUeContent = '{0}',", gm.bUeContent);
                }
            }


            //(8)
            if (!string.IsNullOrEmpty(gm.bSMSOriginalNumLen))
            {
                if (gm.bSMSOriginalNumLen.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bSMSOriginalNumLen = '{0}',", gm.bSMSOriginalNumLen);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(gm.bSMSOriginalNum))
            {
                if (gm.bSMSOriginalNum.Length > 32)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bSMSOriginalNum = '{0}',", gm.bSMSOriginalNum);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(gm.bSMSContentLen))
            {
                if (gm.bSMSContentLen.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bSMSContentLen = '{0}',", gm.bSMSContentLen);
                }
            }

            //(11)
            if (!string.IsNullOrEmpty(gm.bSMSContent))
            {
                if (gm.bSMSContent.Length > 256)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bSMSContent = '{0}',", gm.bSMSContent);
                }
            }


            //(1)
            if (!string.IsNullOrEmpty(gm.SCTP))
            {
                if (gm.SCTP != "0" && gm.SCTP != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("SCTP = {0},", gm.SCTP);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gm.S1))
            {
                if (gm.S1 != "0" && gm.S1 != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("S1 = {0},", gm.S1);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gm.GPS))
            {
                if (gm.GPS != "0" && gm.GPS != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("GPS = {0},", gm.GPS);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gm.CELL))
            {
                if (gm.CELL != "0" && gm.CELL != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("CELL = {0},", gm.CELL);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(gm.SYNC))
            {
                if (gm.SYNC != "0" && gm.SYNC != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("SYNC = {0},", gm.SYNC);
                }
            }

            //(6)
            if (!string.IsNullOrEmpty(gm.LICENSE))
            {
                if (gm.LICENSE != "0" && gm.LICENSE != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("LICENSE = {0},", gm.LICENSE);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(gm.RADIO))
            {
                if (gm.RADIO != "0" && gm.RADIO != "1")
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
                else
                {
                    sqlSub += string.Format("RADIO = {0},", gm.RADIO);
                }
            }

            //(8)
            if (!string.IsNullOrEmpty(gm.time))
            {
                try
                {
                    DateTime.Parse(gm.time);
                    sqlSub += string.Format("time = '{0}',", gm.time);
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    sqlSub += string.Format("time = '{0}',", DateTime.Now.ToString());
                }
            }


            //(1,2)
            if (!string.IsNullOrEmpty(gm.activeTime1Start) && !string.IsNullOrEmpty(gm.activeTime1Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(gm.activeTime1Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(gm.activeTime1Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime1Start = '{0}',activeTime1Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(3,4)
            if (!string.IsNullOrEmpty(gm.activeTime2Start) && !string.IsNullOrEmpty(gm.activeTime2Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(gm.activeTime2Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(gm.activeTime2Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime2Start = '{0}',activeTime2Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }


            //(5,6)
            if (!string.IsNullOrEmpty(gm.activeTime3Start) && !string.IsNullOrEmpty(gm.activeTime3Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(gm.activeTime3Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(gm.activeTime3Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime3Start = '{0}',activeTime3Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }

            //(7,8)
            if (!string.IsNullOrEmpty(gm.activeTime4Start) && !string.IsNullOrEmpty(gm.activeTime4Ended))
            {
                try
                {
                    string dt1 = DateTime.Parse(gm.activeTime4Start).ToString("HH:mm:ss");
                    string dt2 = DateTime.Parse(gm.activeTime4Ended).ToString("HH:mm:ss");

                    if (string.Compare(dt2, dt1) > 0)
                    {
                        sqlSub += string.Format("activeTime4Start = '{0}',activeTime4Ended = '{1}',", dt1, dt2);
                    }
                    else
                    {
                        Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.TIME_FMT_ERR], "DB", LogCategory.I);
                        return (int)RC.TIME_FMT_ERR;
                    }
                }
                catch (Exception ee)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_FMT_ERR] + " " + ee.Message, "DB", LogCategory.I);
                    return (int)RC.PAR_FMT_ERR;
                }
            }



            //(28)
            if (!string.IsNullOrEmpty(gm.bindingDevId))
            {
                if (gm.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", gm.bindingDevId);
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gc_misc set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gc_misc记录
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gpr">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_misc_record_get_by_devid(int carry, int affDeviceId, ref strGcMisc gm)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_misc_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            gm = new strGcMisc();
            string sql = string.Format("select * from gc_misc where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            //(1)
                            if (!string.IsNullOrEmpty(dr["wTraceLen"].ToString()))
                            {
                                gm.wTraceLen = dr["wTraceLen"].ToString();
                            }

                            //(2)
                            if (!string.IsNullOrEmpty(dr["cTrace"].ToString()))
                            {
                                gm.cTrace = dr["cTrace"].ToString();
                            }


                            //(3)
                            if (!string.IsNullOrEmpty(dr["bOrmType"].ToString()))
                            {
                                gm.bOrmType = dr["bOrmType"].ToString();
                            }

                            //(4)
                            if (!string.IsNullOrEmpty(dr["bUeId"].ToString()))
                            {
                                gm.bUeId = dr["bUeId"].ToString();
                            }

                            //(5)
                            if (!string.IsNullOrEmpty(dr["cRSRP"].ToString()))
                            {
                                gm.cRSRP = dr["cRSRP"].ToString();
                            }

                            //(6)
                            if (!string.IsNullOrEmpty(dr["bUeContentLen"].ToString()))
                            {
                                gm.bUeContentLen = dr["bUeContentLen"].ToString();
                            }

                            //(7)
                            if (!string.IsNullOrEmpty(dr["bUeContent"].ToString()))
                            {
                                gm.bUeContent = dr["bUeContent"].ToString();
                            }


                            //(8)
                            if (!string.IsNullOrEmpty(dr["bSMSOriginalNumLen"].ToString()))
                            {
                                gm.bSMSOriginalNumLen = dr["bSMSOriginalNumLen"].ToString();
                            }

                            //(9)
                            if (!string.IsNullOrEmpty(dr["bSMSOriginalNum"].ToString()))
                            {
                                gm.bSMSOriginalNum = dr["bSMSOriginalNum"].ToString();
                            }

                            //(10)
                            if (!string.IsNullOrEmpty(dr["bSMSContentLen"].ToString()))
                            {
                                gm.bSMSContentLen = dr["bSMSContentLen"].ToString();
                            }

                            //(11)
                            if (!string.IsNullOrEmpty(dr["bSMSContent"].ToString()))
                            {
                                gm.bSMSContent = dr["bSMSContent"].ToString();
                            }


                            //(1)
                            if (!string.IsNullOrEmpty(dr["SCTP"].ToString()))
                            {
                                gm.SCTP = dr["SCTP"].ToString();
                            }

                            //(2)
                            if (!string.IsNullOrEmpty(dr["S1"].ToString()))
                            {
                                gm.S1 = dr["S1"].ToString();
                            }

                            //(3)
                            if (!string.IsNullOrEmpty(dr["GPS"].ToString()))
                            {
                                gm.GPS = dr["GPS"].ToString();
                            }

                            //(4)
                            if (!string.IsNullOrEmpty(dr["CELL"].ToString()))
                            {
                                gm.CELL = dr["CELL"].ToString();
                            }

                            //(5)
                            if (!string.IsNullOrEmpty(dr["SYNC"].ToString()))
                            {
                                gm.SYNC = dr["SYNC"].ToString();
                            }

                            //(6)
                            if (!string.IsNullOrEmpty(dr["LICENSE"].ToString()))
                            {
                                gm.LICENSE = dr["LICENSE"].ToString();
                            }

                            //(7)
                            if (!string.IsNullOrEmpty(dr["RADIO"].ToString()))
                            {
                                gm.RADIO = dr["RADIO"].ToString();
                            }

                            //(8)
                            if (!string.IsNullOrEmpty(dr["time"].ToString()))
                            {
                                gm.time = dr["time"].ToString();
                            }


                            //(1)
                            if (!string.IsNullOrEmpty(dr["activeTime1Start"].ToString()))
                            {
                                gm.activeTime1Start = dr["activeTime1Start"].ToString();
                            }

                            //(2)
                            if (!string.IsNullOrEmpty(dr["activeTime1Ended"].ToString()))
                            {
                                gm.activeTime1Ended = dr["activeTime1Ended"].ToString();
                            }

                            //(3)
                            if (!string.IsNullOrEmpty(dr["activeTime2Start"].ToString()))
                            {
                                gm.activeTime2Start = dr["activeTime2Start"].ToString();
                            }

                            //(4)
                            if (!string.IsNullOrEmpty(dr["activeTime2Ended"].ToString()))
                            {
                                gm.activeTime2Ended = dr["activeTime2Ended"].ToString();
                            }

                            //(5)
                            if (!string.IsNullOrEmpty(dr["activeTime3Start"].ToString()))
                            {
                                gm.activeTime3Start = dr["activeTime3Start"].ToString();
                            }

                            //(6)
                            if (!string.IsNullOrEmpty(dr["activeTime3Ended"].ToString()))
                            {
                                gm.activeTime3Ended = dr["activeTime3Ended"].ToString();
                            }

                            //(7)
                            if (!string.IsNullOrEmpty(dr["activeTime4Start"].ToString()))
                            {
                                gm.activeTime4Start = dr["activeTime4Start"].ToString();
                            }

                            //(8)
                            if (!string.IsNullOrEmpty(dr["activeTime4Ended"].ToString()))
                            {
                                gm.activeTime4Ended = dr["activeTime4Ended"].ToString();
                            }

                            //(28)
                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gm.bindingDevId = dr["bindingDevId"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gc_misc表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_misc_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_misc_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gc_misc where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 25-gc_imsi_action操作

        /// <summary>
        /// 检查gc_imsi_action记录是否存在
        /// </summary>
        /// <param name="bIMSI">imsi</param>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.CARRY_ERR    ：载波非法
        ///   RC.NO_EXIST     ：不存在
        ///   RC.EXIST        ：存在
        /// </returns>
        public int gc_imsi_action_record_exist(string bIMSI, int carry, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(bIMSI))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (bIMSI.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("select count(*) from gc_imsi_action where bIMSI = '{0}' and carry = {1} and affDeviceId = {2}", bIMSI, carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gc_imsi_action表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_imsi_action_record_insert(int carry, int affDeviceId, strGcImsiAction gia)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(gia.bIMSI) || string.IsNullOrEmpty(gia.bUeActionFlag))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (gia.bIMSI.Length > 15 || gia.bUeActionFlag.Length > 2)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gc_imsi_action_record_exist(gia.bIMSI, carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("insert into gc_imsi_action(id,bIMSI,bUeActionFlag,carry,bindingDevId,affDeviceId) values(NULL,'{0}','{1}',{2},-1,{3})", gia.bIMSI, gia.bUeActionFlag, carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gc_imsi_action所有记录
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gpr">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_imsi_action_record_get_by_devid(int carry, int affDeviceId, ref List<strGcImsiAction> list)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            list = new List<strGcImsiAction>();
            string sql = string.Format("select * from gc_imsi_action where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            strGcImsiAction str = new strGcImsiAction();
                            if (!string.IsNullOrEmpty(dr["bIMSI"].ToString()))
                            {
                                str.bIMSI = dr["bIMSI"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bUeActionFlag"].ToString()))
                            {
                                str.bUeActionFlag = dr["bUeActionFlag"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res1"].ToString()))
                            {
                                str.res1 = dr["res1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res2"].ToString()))
                            {
                                str.res2 = dr["res2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["res3"].ToString()))
                            {
                                str.res3 = dr["res3"].ToString();
                            }

                            list.Add(str);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gc_imsi_action表中删除指定的所有记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_imsi_action_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            string sql = string.Format("delete from gc_imsi_action where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gc_imsi_action表中删除指定的所有记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_imsi_action_record_delete(int carry, int affDeviceId, string bIMSI)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (string.IsNullOrEmpty(bIMSI))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_NULL], "DB", LogCategory.I);
                return (int)RC.PAR_NULL;
            }

            if (bIMSI.Length > 15)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                return (int)RC.PAR_LEN_ERR;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            string sql = string.Format("delete from gc_imsi_action where bIMSI = '{0}' and carry = {1} and affDeviceId = {2}", bIMSI, carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion

        #region 26-gc_carrier_msg操作

        /// <summary>
        /// 检查gc_carrier_msg记录是否存在
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属的设备ID好</param>
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.DEV_NO_EXIST ：设备不存在
        ///   RC.CARRY_ERR    ：载波非法
        ///   RC.NO_EXIST     ：不存在
        ///   RC.EXIST        ：存在
        /// </returns>
        public int gc_carrier_msg_record_exist(int carry, int affDeviceId)
        {
            UInt32 cnt = 0;

            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            string sql = string.Format("select count(*) from gc_carrier_msg where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cnt = Convert.ToUInt32(dr[0]);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            if (cnt > 0)
            {
                return (int)RC.EXIST;
            }
            else
            {
                return (int)RC.NO_EXIST;
            }
        }

        /// <summary>
        /// 插入记录到gc_carrier_msg表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.DEV_NO_EXIST   ：设备不存在
        ///   RC.EXIST          ：记录已经存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_carrier_msg_record_insert(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查设备是否存在
            if ((int)RC.NO_EXIST == device_record_exist(affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.DEV_NO_EXIST], "DB", LogCategory.I);
                return (int)RC.DEV_NO_EXIST;
            }

            //检查记录是否存在
            if ((int)RC.EXIST == gc_carrier_msg_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.EXIST], "DB", LogCategory.I);
                return (int)RC.EXIST;
            }

            string sql = string.Format("insert into gc_carrier_msg(id,carry,bindingDevId,affDeviceId) values(NULL,{0},-1,{1})", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 更新记录到gc_carrier_msg表中
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gcm">
        /// 要更新的结构体，那些字段不为空就更新那些
        /// </param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.PAR_NULL       ：参数为空
        ///   PAR_LEN_ERR       ：参数长度有误
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.PAR_FMT_ERR    ：参数格式有误
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_carrier_msg_record_update(int carry, int affDeviceId, strGcCarrierMsg gcm)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_carrier_msg_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sqlSub = "";

            //(1)
            if (!string.IsNullOrEmpty(gcm.wARFCN1))
            {
                if (gcm.wARFCN1.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN1 = '{0}',", gcm.wARFCN1);
                }
            }

            //(2)
            if (!string.IsNullOrEmpty(gcm.bARFCN1Mode))
            {
                if (gcm.bARFCN1Mode.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bARFCN1Mode = '{0}',", gcm.bARFCN1Mode);
                }
            }

            //(3)
            if (!string.IsNullOrEmpty(gcm.bReserved1))
            {
                if (gcm.bReserved1.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bReserved1 = '{0}',", gcm.bReserved1);
                }
            }

            //(4)
            if (!string.IsNullOrEmpty(gcm.wARFCN1Duration))
            {
                if (gcm.wARFCN1Duration.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN1Duration = '{0}',", gcm.wARFCN1Duration);
                }
            }

            //(5)
            if (!string.IsNullOrEmpty(gcm.wARFCN1Period))
            {
                if (gcm.wARFCN1Period.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN1Period = '{0}',", gcm.wARFCN1Period);
                }
            }





            //(6)
            if (!string.IsNullOrEmpty(gcm.wARFCN2))
            {
                if (gcm.wARFCN2.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN2 = '{0}',", gcm.wARFCN2);
                }
            }

            //(7)
            if (!string.IsNullOrEmpty(gcm.bARFCN2Mode))
            {
                if (gcm.bARFCN2Mode.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bARFCN2Mode = '{0}',", gcm.bARFCN2Mode);
                }
            }


            //(8)
            if (!string.IsNullOrEmpty(gcm.bReserved2))
            {
                if (gcm.bReserved2.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bReserved2 = '{0}',", gcm.bReserved2);
                }
            }

            //(9)
            if (!string.IsNullOrEmpty(gcm.wARFCN2Duration))
            {
                if (gcm.wARFCN2Duration.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN2Duration = '{0}',", gcm.wARFCN2Duration);
                }
            }

            //(10)
            if (!string.IsNullOrEmpty(gcm.wARFCN2Period))
            {
                if (gcm.wARFCN2Period.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN2Period = '{0}',", gcm.wARFCN2Period);
                }
            }




            //(11)
            if (!string.IsNullOrEmpty(gcm.wARFCN3))
            {
                if (gcm.wARFCN3.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN3 = '{0}',", gcm.wARFCN3);
                }
            }


            //(12)
            if (!string.IsNullOrEmpty(gcm.bARFCN3Mode))
            {
                if (gcm.bARFCN3Mode.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bARFCN3Mode = '{0}',", gcm.bARFCN3Mode);
                }
            }


            //(13)
            if (!string.IsNullOrEmpty(gcm.bReserved3))
            {
                if (gcm.bReserved3.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bReserved3 = '{0}',", gcm.bReserved3);
                }
            }

            //(14)
            if (!string.IsNullOrEmpty(gcm.wARFCN3Duration))
            {
                if (gcm.wARFCN3Duration.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN3Duration = '{0}',", gcm.wARFCN3Duration);
                }
            }


            //(15)
            if (!string.IsNullOrEmpty(gcm.wARFCN3Period))
            {
                if (gcm.wARFCN3Period.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN3Period = '{0}',", gcm.wARFCN3Period);
                }
            }



            //(16)
            if (!string.IsNullOrEmpty(gcm.wARFCN4))
            {
                if (gcm.wARFCN4.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN4 = '{0}',", gcm.wARFCN4);
                }
            }

            //(17)
            if (!string.IsNullOrEmpty(gcm.bARFCN4Mode))
            {
                if (gcm.bARFCN4Mode.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bARFCN4Mode = '{0}',", gcm.bARFCN4Mode);
                }
            }


            //(18)
            if (!string.IsNullOrEmpty(gcm.bReserved4))
            {
                if (gcm.bReserved4.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bReserved4 = '{0}',", gcm.bReserved4);
                }
            }

            //(19)
            if (!string.IsNullOrEmpty(gcm.wARFCN4Duration))
            {
                if (gcm.wARFCN4Duration.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN4Duration = '{0}',", gcm.wARFCN4Duration);
                }
            }

            //(20)
            if (!string.IsNullOrEmpty(gcm.wARFCN4Period))
            {
                if (gcm.wARFCN4Period.Length > 8)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("wARFCN4Period = '{0}',", gcm.wARFCN4Period);
                }
            }

            //(21)
            if (!string.IsNullOrEmpty(gcm.bindingDevId))
            {
                if (gcm.bindingDevId.Length > 11)
                {
                    Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.PAR_LEN_ERR], "DB", LogCategory.I);
                    return (int)RC.PAR_LEN_ERR;
                }
                else
                {
                    sqlSub += string.Format("bindingDevId = {0},", gcm.bindingDevId);
                }
            }

            if (sqlSub != "")
            {
                //去掉最后一个字符
                sqlSub = sqlSub.Remove(sqlSub.Length - 1, 1);
            }
            else
            {
                //不需要更新
                Logger.Trace(LogInfoType.INFO, "无需更新", "DB", LogCategory.I);
                return (int)RC.SUCCESS;
            }

            string sql = string.Format("update gc_carrier_msg set {0} where carry = {1} and affDeviceId = {2}", sqlSub, carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.WARN, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 通过设备ID号获取gc_carrier_msg记录
        /// </summary>
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>
        /// <param name="gcm">affDeviceId对应的详细信息</param>
        /// <returns>
        ///   RC.NO_OPEN        ：数据库尚未打开
        ///   RC.OP_FAIL        ：数据库操作失败 
        ///   RC.NO_EXIST       ：记录不存在
        ///   RC.SUCCESS        ：成功 
        /// </returns>
        public int gc_carrier_msg_record_get_by_devid(int carry, int affDeviceId, ref strGcCarrierMsg gcm)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_carrier_msg_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            gcm = new strGcCarrierMsg();
            string sql = string.Format("select * from gc_carrier_msg where carry = {0} and affDeviceId = {1}", carry, affDeviceId);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {                            
                            if (!string.IsNullOrEmpty(dr["wARFCN1"].ToString()))
                            {
                                gcm.wARFCN1 = dr["wARFCN1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bARFCN1Mode"].ToString()))
                            {
                                gcm.bARFCN1Mode = dr["bARFCN1Mode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bReserved1"].ToString()))
                            {
                                gcm.bReserved1 = dr["bReserved1"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN1Duration"].ToString()))
                            {
                                gcm.wARFCN1Duration = dr["wARFCN1Duration"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN1Period"].ToString()))
                            {
                                gcm.wARFCN1Period = dr["wARFCN1Period"].ToString();
                            }




                            if (!string.IsNullOrEmpty(dr["wARFCN2"].ToString()))
                            {
                                gcm.wARFCN2 = dr["wARFCN2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bARFCN2Mode"].ToString()))
                            {
                                gcm.bARFCN2Mode = dr["bARFCN2Mode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bReserved2"].ToString()))
                            {
                                gcm.bReserved2 = dr["bReserved2"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN2Duration"].ToString()))
                            {
                                gcm.wARFCN2Duration = dr["wARFCN2Duration"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN2Period"].ToString()))
                            {
                                gcm.wARFCN2Period = dr["wARFCN2Period"].ToString();
                            }



                            if (!string.IsNullOrEmpty(dr["wARFCN3"].ToString()))
                            {
                                gcm.wARFCN3 = dr["wARFCN3"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bARFCN3Mode"].ToString()))
                            {
                                gcm.bARFCN3Mode = dr["bARFCN3Mode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bReserved3"].ToString()))
                            {
                                gcm.bReserved3 = dr["bReserved3"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN3Duration"].ToString()))
                            {
                                gcm.wARFCN3Duration = dr["wARFCN3Duration"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN3Period"].ToString()))
                            {
                                gcm.wARFCN3Period = dr["wARFCN3Period"].ToString();
                            }



                            if (!string.IsNullOrEmpty(dr["wARFCN4"].ToString()))
                            {
                                gcm.wARFCN4 = dr["wARFCN4"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bARFCN4Mode"].ToString()))
                            {
                                gcm.bARFCN4Mode = dr["bARFCN4Mode"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["bReserved4"].ToString()))
                            {
                                gcm.bReserved4 = dr["bReserved4"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN4Duration"].ToString()))
                            {
                                gcm.wARFCN4Duration = dr["wARFCN4Duration"].ToString();
                            }

                            if (!string.IsNullOrEmpty(dr["wARFCN4Period"].ToString()))
                            {
                                gcm.wARFCN4Period = dr["wARFCN4Period"].ToString();
                            }

                           
                            if (!string.IsNullOrEmpty(dr["bindingDevId"].ToString()))
                            {
                                gcm.bindingDevId = dr["bindingDevId"].ToString();
                            }
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        /// <summary>
        /// 在gc_carrier_msg表中删除指定的记录 
        /// </summary>  
        /// <param name="carry">载波ID，对于CDMA固定为-1，GSM-V2的0或1</param>
        /// <param name="affDeviceId">所属设备ID</param>    
        /// <returns>
        ///   RC.NO_OPEN      ：数据库尚未打开
        ///   RC.OP_FAIL      ：数据库操作失败 
        ///   RC.NO_EXIST     ：记录不存在
        ///   RC.SUCCESS      ：成功
        /// </returns>
        public int gc_carrier_msg_record_delete(int carry, int affDeviceId)
        {
            if (false == myDbConnFlag)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_OPEN], "DB", LogCategory.I);
                return (int)RC.NO_OPEN;
            }

            if (carry != -1 && carry != 0 && carry != 1)
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.CARRY_ERR], "DB", LogCategory.I);
                return (int)RC.CARRY_ERR;
            }

            //检查记录是否存在
            if ((int)RC.NO_EXIST == gc_carrier_msg_record_exist(carry, affDeviceId))
            {
                Logger.Trace(LogInfoType.EROR, dicRTV[(int)RC.NO_EXIST], "DB", LogCategory.I);
                return (int)RC.NO_EXIST;
            }

            string sql = string.Format("delete from gc_carrier_msg where carry = {0} and affDeviceId = {1}", carry, affDeviceId);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, myDbConn))
                {
                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        Logger.Trace(LogInfoType.EROR, sql, "DB", LogCategory.I);
                        return (int)RC.OP_FAIL;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace(LogInfoType.EROR, e.Message, "DB", LogCategory.I);
                return (int)RC.OP_FAIL;
            }

            return (int)RC.SUCCESS;
        }

        #endregion               
    }
}
