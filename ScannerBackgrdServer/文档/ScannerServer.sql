
/*******************************************************************************
   
     (1) modify the password of root
     (2) enable root to log DB in remotely
     (3) create DB of scanner_server
     (4) create various tables.
      
                            2018-04-24
     (5) add index.    
                            2018-05-27                         
                            
     (6) add field cellid to ap_general_para
                            2018-06-26
                            
     (7) add table redirection.
                            2018-06-28                   
     
     (8) add activeTime Start and end for GSM
         TAC:smallint->smallint unsigned
                            2018-07-18                      
                                                                                                                                          
********************************************************************************/



use mysql;
update user set password=PASSWORD("root") where user='root';
FLUSH PRIVILEGES;

GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY 'root' WITH GRANT OPTION;
FLUSH PRIVILEGES;
		
create database scanner_server;
use scanner_server;


/* === (1) user === */
SELECT '-----user prcess--------';


/* DROP TABLE IF EXISTS user; */
CREATE TABLE user 
(  
	usrId       int           unsigned    primary key NOT NULL auto_increment COMMENT '主键ID',  
	name        char(64)      NOT NULL  COMMENT '用户名',  
	psw         BLOB          NOT NULL  COMMENT '用户密码',
	des         varchar(256)  NULL default '' COMMENT '描述',
	operTime    datetime      NOT NULL  COMMENT '创建时间或最近一次的登录时间'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 

DELETE from user where name = 'engi';
DELETE from user where name = 'root';

INSERT INTO user VALUES(NULL,'engi','MD5(engi)','engi(默认存在)',NOW());
INSERT INTO user VALUES(NULL,'root','MD5(root)','root(默认存在)',NOW());



/* === (2) roletype === */
SELECT '-----roletype prcess--------';


DROP TABLE IF EXISTS roletype;
CREATE TABLE roletype 
(  
	id          tinyint       unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',  
	roleType    char(64)      NOT NULL  COMMENT '角色类型，默认有Engineering,SuperAdmin,Administrator,SeniorOperator,Operator,还可以自定义',  
	des         varchar(256)  NULL default 'des' COMMENT '描述'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 

INSERT INTO roletype VALUES(1,'Engineering'   ,'Engineering(默认存在)');
INSERT INTO roletype VALUES(2,'SuperAdmin'    ,'SuperAdmin(默认存在)');
INSERT INTO roletype VALUES(3,'Administrator' ,'Administrator(默认存在)');
INSERT INTO roletype VALUES(4,'SeniorOperator','SeniorOperator(默认存在)');
INSERT INTO roletype VALUES(5,'Operator'      ,'Operator(默认存在)');


/* === (3) role === */
SELECT '-----role prcess--------';


DROP TABLE IF EXISTS role;
CREATE TABLE role 
(  
	roleId      int           unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',  
	name        char(64)      NOT NULL  COMMENT '角色名称',  
	roleType    char(64)      NOT NULL  COMMENT '角色类型，默认有Engineering,SuperAdmin,Administrator,SeniorOperator,Operator,还可以自定义',  
	timeStart   datetime      NOT NULL  COMMENT '开始的生效时间',
	timeEnd     datetime      NOT NULL  COMMENT '结束的生效时间',
	des         varchar(256)  NULL default 'des' COMMENT '描述'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 

	                
INSERT INTO role VALUES(NULL,'RoleEng'  ,'Engineering'   ,'1970-1-1','3000-1-1','RoleEng(默认存在)');
INSERT INTO role VALUES(NULL,'RoleSA'   ,'SuperAdmin'    ,'1970-1-1','3000-1-1','RoleSA(默认存在)');
INSERT INTO role VALUES(NULL,'RoleAdmin','Administrator' ,'1970-1-1','3000-1-1','RoleAdmin(默认存在)');
INSERT INTO role VALUES(NULL,'RoleSO'   ,'SeniorOperator','1970-1-1','3000-1-1','RoleSO(默认存在)');
INSERT INTO role VALUES(NULL,'RoleOP'   ,'Operator'      ,'1970-1-1','3000-1-1','RoleOP(默认存在)');



/* === (4) privilege === */
SELECT '-----privilege prcess--------';


DROP TABLE IF EXISTS privilege;
CREATE TABLE privilege
(
	priId        int            unsigned primary key NOT NULL auto_increment COMMENT '主键ID',
	funName      char(64)       NOT NULL COMMENT '功能名称',
	aliasName    char(64)       NOT NULL COMMENT '功能别名',	
	des          varchar(256)   NULL default 'des' COMMENT '描述'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;      



/* === (5) userrole === */
SELECT '-----userrole prcess--------';


DROP TABLE IF EXISTS userrole;
CREATE TABLE userrole
(
	usrRoleId    int            unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',
	usrName      char(64)       NOT NULL COMMENT '用户名，FK',
	roleName     char(64)       NOT NULL COMMENT '角色名，FK',	
	des          varchar(256)   NULL default 'des' COMMENT '描述'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;   

INSERT INTO userrole VALUES(1,'engi' ,'RoleEng' ,'engi->RoleEng(默认存在)');
INSERT INTO userrole VALUES(2,'root' ,'RoleSA'  ,'root->RoleSA(默认存在)');        


/* === (6) roleprivilege === */
SELECT '-----roleprivilege prcess--------';


DROP TABLE IF EXISTS roleprivilege;
CREATE TABLE roleprivilege
(
	rolePriId    int            unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',
	roleName     char(64)       NOT NULL COMMENT '角色名，FK',	
	priIdSet     varchar(1024)  NULL default '' COMMENT '权限ID集合',	
	des          varchar(256)   NULL default 'des' COMMENT '描述'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;



/* === (7) domain === */
SELECT '-----domain prcess--------';

DROP TABLE IF EXISTS domain; 
CREATE TABLE domain
(
	id            int            primary key NOT NULL auto_increment COMMENT '主键ID',
	name          char(64)       NOT NULL COMMENT '节点的名称',	
	parentId      int            NOT NULL COMMENT '节点的父亲ID',	
	nameFullPath  varchar(1024)  NOT NULL COMMENT '节点的名称全路径',	
	isStation     tinyint        NOT NULL COMMENT '标识是否为站点',	
	des           varchar(256)   NULL default 'des' COMMENT '描述'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;
 
 
INSERT INTO domain VALUES(1,'设备',-1,'设备',0,'根节点(默认存在)'); 



/* === (8) userdomain === */
SELECT '-----userdomain prcess--------';

/* DROP TABLE IF EXISTS userdomain; */
CREATE TABLE userdomain
(
	usrDomainId   int            unsigned primary key NOT NULL auto_increment COMMENT '主键ID',
	usrName       char(64)       NOT NULL COMMENT '用户名',		
	domainIdSet   varchar(1024)  NOT NULL COMMENT '域ID集合，如1,2,3,4,5',		
	des           varchar(256)   NULL default 'des' COMMENT '描述'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;



/* === (9) device === */
SELECT '-----device prcess--------';

CREATE TABLE device 
(  
	id             int                   unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',
	name           char(64)  	         NOT NULL  COMMENT '设备名称',
	sn             char(32)  	         NULL default NULL COMMENT 'SN，GSM或第三方设备可能没有该字段',		
	ipAddr         char(32)  	         NULL default NULL COMMENT 'IP地址',
	port           smallint    unsigned  NULL default NULL COMMENT '端口号',
	netmask        char(16)  	         NULL default NULL COMMENT '掩码',
	mode           char(16)              NULL COMMENT '设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等', 
	online         tinyint               NULL default 0    COMMENT '上下线标识，0：下线；1：上线',
	lastOnline     datetime              NULL default NULL COMMENT '最后的上线时间',
	isActive       tinyint               NULL default 1    COMMENT '标识该设备是否生效，0：无效；1：生效',
	innerType      char(16)              NULL default NULL COMMENT '用于软件内部处理', 
	affDomainId    int              	 NOT NULL COMMENT '标识设备的从属于那个域，FK'
		
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 

CREATE INDEX inx_device ON device (name,sn,affDomainId);


/* === (10) device_unknown === */
SELECT '-----device_unknown prcess--------';

CREATE TABLE device_unknown 
(  
	id             int                   unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',
	name           char(64)  	         NOT NULL  COMMENT '设备名称',
	sn             char(32)  	         NULL default NULL COMMENT 'SN，GSM或第三方设备可能没有该字段',		
	ipAddr         char(32)  	         NULL default NULL COMMENT 'IP地址',
	port           smallint    unsigned  NULL default NULL COMMENT '端口号',
	netmask        char(16)  	         NULL default NULL COMMENT '掩码',
	mode           char(16)              NULL COMMENT '设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等', 
	online         tinyint               NULL default 0    COMMENT '上下线标识，0：下线；1：上线',
	lastOnline     datetime              NULL default NULL COMMENT '最后的上线时间',
	isActive       tinyint               NULL default 1    COMMENT '标识该设备是否生效，0：无效；1：生效',
	innerType      char(16)              NULL default NULL COMMENT '用于软件内部处理', 
	affDomainId    int              	 NOT NULL COMMENT '标识设备的从属于那个域，FK'
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;

CREATE INDEX inx_device_unkown ON device (name,sn,affDomainId);


/* === (10) bwlist === */
SELECT '-----bwlist prcess--------';

CREATE TABLE bwlist 
(  
	id               int      unsigned     primary key NOT NULL auto_increment COMMENT '主键ID',
	imsi             char(15)  	           NULL  COMMENT 'IMSI号',
	imei             char(15)  	           NULL  COMMENT 'IMEI号',
	bwFlag           enum('white','black','other') DEFAULT 'white' COMMENT '名单类型标识',
	rbStart          tinyint      	       NULL default 0  COMMENT '起始RB',
	rbEnd            tinyint      	       NULL default 0  COMMENT '结束RB',
	time             datetime              NULL COMMENT '设置时间',
	des              varchar(128)  	       NULL default '' COMMENT '描述',
	linkFlag         tinyint      	       NOT NULL default 0  COMMENT '0：链接到DeviceId,1：链接到DomainId',
	affDeviceId      int                   NULL default 0 COMMENT '所属的设备ID号；即关联到device表,FK',
	affDomainId      int              	   NULL default 0 COMMENT '所属的域的ID号；即关联到domain表,FK' 	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;

CREATE INDEX inx_bwlist ON bwlist (imsi,imei,bwFlag,affDeviceId,affDomainId);


/* === (12) capture === */
SELECT '-----capture prcess--------';

CREATE TABLE capture 
(  
	id             bigint   unsigned     primary key NOT NULL auto_increment COMMENT '主键ID,0~18446744073709551615',
	imsi           char(15)  	         NULL  COMMENT 'IMSI号',
	imei           char(15)  	         NULL  COMMENT 'IMEI号',
	bwFlag         enum('white','black','other') DEFAULT 'white' COMMENT '名单类型标识',
	isdn           int                   NULL  COMMENT '手机号码号段',
	bsPwr          tinyint      	     NULL  COMMENT '手机上报的基站功率',
	tmsi           int                   NULL  COMMENT '手机TMSI号',
	time           datetime              NOT NULL  COMMENT '感知时间',
	affDeviceId    int                   NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;

CREATE INDEX inx_capture ON capture (imsi,imei,bwFlag,time,affDeviceId);


/* === (13) gsm_report_white_list === */
SELECT '-----gsm_report_white_list prcess--------';

CREATE TABLE gsm_report_white_list 
(  
	id               int      unsigned   primary key NOT NULL auto_increment COMMENT '主键ID',
	imsi             char(15)  	         NOT NULL  COMMENT 'IMSI号',
	time             datetime            NOT NULL  COMMENT '感知时间',	
	forwardingFlag   tinyint      	     NULL  COMMENT '是否已经转发给绑定的LTE设备,0:尚未转发 1:已经转发',
	forwardingStat   tinyint      	     NULL  COMMENT '转发个绑定LTE设备的状态,0:尚未查询转发状态 1:转发失败 2:转发成功',	
	carry            tinyint             NULL default -1   COMMENT 'GSM的载波标识0或1',
	bindingDevId     int                 NULL default -1   COMMENT '仅用于标识GSM的绑定设备id', 	
	affDeviceId      int                 NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;


/* === (14) gsm_rf_para === */
SELECT '-----gsm_rf_para prcess--------';

CREATE TABLE gsm_rf_para 
(  
	id                 int      unsigned   primary key NOT NULL auto_increment COMMENT '主键ID',	
	rfEnable           tinyint      	   NULL default 0   COMMENT '射频使能,0表示射频关闭，1表示射频打开',
	rfFreq             smallint      	   NULL default 75  COMMENT '信道号',	
    rfPwr              tinyint      	   NULL default 63  COMMENT '发射功率衰减值',
    carry              tinyint             NULL default -1  COMMENT 'GSM的载波标识0或1',
	bindingDevId       int                 NULL default -1  COMMENT '仅用于标识GSM的绑定设备id', 	
	activeTime1Start   time                NULL  COMMENT '生效时间1的起始时间',
	activeTime1Ended   time                NULL  COMMENT '生效时间1的结束时间',
	activeTime2Start   time                NULL  COMMENT '生效时间2的起始时间',
	activeTime2Ended   time                NULL  COMMENT '生效时间2的结束时间',
	activeTime3Start   time                NULL  COMMENT '生效时间3的起始时间,有的话就添加该项',
	activeTime3Ended   time                NULL  COMMENT '生效时间3的结束时间,有的话就添加该项',
	activeTime4Start   time                NULL  COMMENT '生效时间4的起始时间,有的话就添加该项',
	activeTime4Ended   time                NULL  COMMENT '生效时间4的结束时间,有的话就添加该项',
	affDeviceId    int                     NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;



/* === (15) gsm_sys_para === */
SELECT '-----gsm_sys_para prcess--------';

CREATE TABLE gsm_sys_para 
(  
	id             int      unsigned   primary key NOT NULL auto_increment COMMENT '主键ID',	
	paraMcc        smallint      	   NULL default 1  COMMENT '移动国家码',
	paraMnc        smallint      	   NULL default 1  COMMENT '移动网号',	
    paraBsic       tinyint      	   NULL default 56 COMMENT '基站识别码',
    paraLac        smallint      	   NULL default 1  COMMENT '位置区号',
    paraCellId     smallint      	   NULL default 10 COMMENT '小区ID',
    paraC2         tinyint      	   NULL default 63 COMMENT 'C2偏移量',
    paraPeri       tinyint      	   NULL default 30 COMMENT '周期性位置更新周期',
    paraAccPwr     tinyint      	   NULL default 5  COMMENT '接入功率',
    paraMsPwr      tinyint      	   NULL default 5  COMMENT '手机发射功率',
    paraRejCau     tinyint      	   NULL default 15 COMMENT '位置更新拒绝原因',
    carry          tinyint             NULL default -1   COMMENT 'GSM的载波标识0或1',
	bindingDevId   int                 NULL default -1   COMMENT '仅用于标识GSM的绑定设备id', 	
	affDeviceId    int                 NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	

	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;



/* === (16) gsm_sys_option === */
SELECT '-----gsm_sys_option prcess--------';

CREATE TABLE gsm_sys_option 
(  
	id             int      unsigned   primary key NOT NULL auto_increment COMMENT '主键ID',	
	opLuSms        tinyint      	   NULL default 0 COMMENT '登录时发送短信',
	opLuImei       tinyint      	   NULL default 1 COMMENT '登录时获取IMEI',	
    opCallEn       tinyint      	   NULL default 0 COMMENT '允许用户主叫',
    opDebug        tinyint      	   NULL default 0 COMMENT '调试模式，上报信令',
    opLuType       tinyint      	   NULL default 2 COMMENT '登录类型',
    opSmsType      tinyint      	   NULL default 2 COMMENT '短信类型',
    opRegModel     tinyint      	   NULL default 0 COMMENT '注册工作模式',    
    carry          tinyint             NULL default -1   COMMENT 'GSM的载波标识0或1',
	bindingDevId   int                 NULL default -1   COMMENT '仅用于标识GSM的绑定设备id', 	
	affDeviceId    int                 NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;



/* === (17) gsm_msg_option === */
SELECT '-----gsm_msg_option prcess--------';

CREATE TABLE gsm_msg_option 
(  
	id              int      unsigned   primary key NOT NULL auto_increment COMMENT '主键ID',	
	smsRPOA         char(20)  	        NULL  COMMENT '短消息中心号码',
	smsTPOA         char(20)  	        NULL  COMMENT '短消息原叫号码',
	smsSCTS         char(20)  	        NULL  COMMENT '短消息发送时间',
	smsDATA         varchar(1024)       NULL  COMMENT '短消息内容',
	autoSend        tinyint      	    NULL  COMMENT '是否自动发送',	
    autoFilterSMS   tinyint      	    NULL  COMMENT '是否自动过滤短信',
    delayTime       int      	        NULL  COMMENT '发送延时时间',
    smsCoding       tinyint      	    NULL  COMMENT '短信的编码格式',    
    carry           tinyint             NULL default -1   COMMENT 'GSM的载波标识0或1',
	bindingDevId    int                 NULL default -1   COMMENT '仅用于标识GSM的绑定设备id', 	
	affDeviceId     int                 NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;




/*
////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////
*/




/* === (18) j_position_provice === */
SELECT '-----j_position_provice prcess--------';

SET FOREIGN_KEY_CHECKS=0;

DROP TABLE IF EXISTS `j_position_provice`;
CREATE TABLE `j_position_provice` 
(
  `id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `provice_id` int(11) unsigned NOT NULL COMMENT '省份id、省份编号',
  `provice_name` char(32) NOT NULL COMMENT '省份名称',
  PRIMARY KEY (`id`),
  UNIQUE KEY `provice_id` (`provice_id`)
  
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8 COMMENT='省份数据库';

/*
INSERT INTO `j_position_provice` VALUES ('1',  '110', '北京市');
INSERT INTO `j_position_provice` VALUES ('2',  '120', '天津市');
INSERT INTO `j_position_provice` VALUES ('3',  '130', '河北省');
INSERT INTO `j_position_provice` VALUES ('4',  '140', '山西省');
INSERT INTO `j_position_provice` VALUES ('5',  '150', '内蒙古自治区');
INSERT INTO `j_position_provice` VALUES ('6',  '210', '辽宁省');
INSERT INTO `j_position_provice` VALUES ('7',  '220', '吉林省');
INSERT INTO `j_position_provice` VALUES ('8',  '230', '黑龙江省');
INSERT INTO `j_position_provice` VALUES ('9',  '310', '上海市');
INSERT INTO `j_position_provice` VALUES ('10', '320', '江苏省');
INSERT INTO `j_position_provice` VALUES ('11', '330', '浙江省');
INSERT INTO `j_position_provice` VALUES ('12', '340', '安徽省');
INSERT INTO `j_position_provice` VALUES ('13', '350', '福建省');
INSERT INTO `j_position_provice` VALUES ('14', '360', '江西省');
INSERT INTO `j_position_provice` VALUES ('15', '370', '山东省');
INSERT INTO `j_position_provice` VALUES ('16', '410', '河南省');
INSERT INTO `j_position_provice` VALUES ('17', '420', '湖北省');
INSERT INTO `j_position_provice` VALUES ('18', '430', '湖南省');
INSERT INTO `j_position_provice` VALUES ('19', '440', '广东省');
INSERT INTO `j_position_provice` VALUES ('20', '450', '广西壮族自治区');
INSERT INTO `j_position_provice` VALUES ('21', '460', '海南省');
INSERT INTO `j_position_provice` VALUES ('22', '500', '重庆市');
INSERT INTO `j_position_provice` VALUES ('23', '510', '四川省');
INSERT INTO `j_position_provice` VALUES ('24', '520', '贵州省');
INSERT INTO `j_position_provice` VALUES ('25', '530', '云南省');
INSERT INTO `j_position_provice` VALUES ('26', '540', '西藏自治区');
INSERT INTO `j_position_provice` VALUES ('27', '610', '陕西省');
INSERT INTO `j_position_provice` VALUES ('28', '620', '甘肃省');
INSERT INTO `j_position_provice` VALUES ('29', '630', '青海省');
INSERT INTO `j_position_provice` VALUES ('30', '640', '宁夏回族自治区');
INSERT INTO `j_position_provice` VALUES ('31', '650', '新疆维吾尔自治区');

*/


/* === (19) j_position_city === */
SELECT '-----j_position_city prcess--------';


DROP TABLE IF EXISTS `j_position_city`;
CREATE TABLE `j_position_city` 
(
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `province_id` int(10) unsigned NOT NULL COMMENT '地级市id',
  `city_id` bigint(20) unsigned NOT NULL COMMENT '县级市id',
  `city_name` char(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `city_id` (`city_id`),
  KEY `province_id` (`province_id`)
  
) ENGINE=InnoDB AUTO_INCREMENT=346 DEFAULT CHARSET=utf8 COMMENT='县级市数据库';


/*
INSERT INTO `j_position_city` VALUES ('1',  '110', '110100000000', '市辖区');
INSERT INTO `j_position_city` VALUES ('2',  '110', '110200000000', '市辖县');
INSERT INTO `j_position_city` VALUES ('3',  '120', '120100000000', '市辖区');
INSERT INTO `j_position_city` VALUES ('4',  '120', '120200000000', '市辖县');
INSERT INTO `j_position_city` VALUES ('5',  '130', '130100000000', '石家庄市');
INSERT INTO `j_position_city` VALUES ('6',  '130', '130200000000', '唐山市');
INSERT INTO `j_position_city` VALUES ('7',  '130', '130300000000', '秦皇岛市');
INSERT INTO `j_position_city` VALUES ('8',  '130', '130400000000', '邯郸市');
INSERT INTO `j_position_city` VALUES ('9',  '130', '130500000000', '邢台市');
INSERT INTO `j_position_city` VALUES ('10', '130', '130600000000', '保定市');
INSERT INTO `j_position_city` VALUES ('11', '130', '130700000000', '张家口市');
INSERT INTO `j_position_city` VALUES ('12', '130', '130800000000', '承德市');
INSERT INTO `j_position_city` VALUES ('13', '130', '130900000000', '沧州市');
INSERT INTO `j_position_city` VALUES ('14', '130', '131000000000', '廊坊市');
INSERT INTO `j_position_city` VALUES ('15', '130', '131100000000', '衡水市');
INSERT INTO `j_position_city` VALUES ('16', '140', '140100000000', '太原市');
INSERT INTO `j_position_city` VALUES ('17', '140', '140200000000', '大同市');
INSERT INTO `j_position_city` VALUES ('18', '140', '140300000000', '阳泉市');
INSERT INTO `j_position_city` VALUES ('19', '140', '140400000000', '长治市');
INSERT INTO `j_position_city` VALUES ('20', '140', '140500000000', '晋城市');
INSERT INTO `j_position_city` VALUES ('21', '140', '140600000000', '朔州市');
INSERT INTO `j_position_city` VALUES ('22', '140', '140700000000', '晋中市');
INSERT INTO `j_position_city` VALUES ('23', '140', '140800000000', '运城市');
INSERT INTO `j_position_city` VALUES ('24', '140', '140900000000', '忻州市');
INSERT INTO `j_position_city` VALUES ('25', '140', '141000000000', '临汾市');
INSERT INTO `j_position_city` VALUES ('26', '140', '141100000000', '吕梁市');
INSERT INTO `j_position_city` VALUES ('27', '150', '150100000000', '呼和浩特市');
INSERT INTO `j_position_city` VALUES ('28', '150', '150200000000', '包头市');
INSERT INTO `j_position_city` VALUES ('29', '150', '150300000000', '乌海市');
INSERT INTO `j_position_city` VALUES ('30', '150', '150400000000', '赤峰市');
INSERT INTO `j_position_city` VALUES ('31', '150', '150500000000', '通辽市');
INSERT INTO `j_position_city` VALUES ('32', '150', '150600000000', '鄂尔多斯市');
INSERT INTO `j_position_city` VALUES ('33', '150', '150700000000', '呼伦贝尔市');
INSERT INTO `j_position_city` VALUES ('34', '150', '150800000000', '巴彦淖尔市');
INSERT INTO `j_position_city` VALUES ('35', '150', '150900000000', '乌兰察布市');
INSERT INTO `j_position_city` VALUES ('36', '150', '152200000000', '兴安盟');
INSERT INTO `j_position_city` VALUES ('37', '150', '152500000000', '锡林郭勒盟');
INSERT INTO `j_position_city` VALUES ('38', '150', '152900000000', '阿拉善盟');
INSERT INTO `j_position_city` VALUES ('39', '210', '210100000000', '沈阳市');
INSERT INTO `j_position_city` VALUES ('40', '210', '210200000000', '大连市');
INSERT INTO `j_position_city` VALUES ('41', '210', '210300000000', '鞍山市');
INSERT INTO `j_position_city` VALUES ('42', '210', '210400000000', '抚顺市');
INSERT INTO `j_position_city` VALUES ('43', '210', '210500000000', '本溪市');
INSERT INTO `j_position_city` VALUES ('44', '210', '210600000000', '丹东市');
INSERT INTO `j_position_city` VALUES ('45', '210', '210700000000', '锦州市');
INSERT INTO `j_position_city` VALUES ('46', '210', '210800000000', '营口市');
INSERT INTO `j_position_city` VALUES ('47', '210', '210900000000', '阜新市');
INSERT INTO `j_position_city` VALUES ('48', '210', '211000000000', '辽阳市');
INSERT INTO `j_position_city` VALUES ('49', '210', '211100000000', '盘锦市');
INSERT INTO `j_position_city` VALUES ('50', '210', '211200000000', '铁岭市');
INSERT INTO `j_position_city` VALUES ('51', '210', '211300000000', '朝阳市');
INSERT INTO `j_position_city` VALUES ('52', '210', '211400000000', '葫芦岛市');
INSERT INTO `j_position_city` VALUES ('53', '220', '220100000000', '长春市');
INSERT INTO `j_position_city` VALUES ('54', '220', '220200000000', '吉林市');
INSERT INTO `j_position_city` VALUES ('55', '220', '220300000000', '四平市');
INSERT INTO `j_position_city` VALUES ('56', '220', '220400000000', '辽源市');
INSERT INTO `j_position_city` VALUES ('57', '220', '220500000000', '通化市');
INSERT INTO `j_position_city` VALUES ('58', '220', '220600000000', '白山市');
INSERT INTO `j_position_city` VALUES ('59', '220', '220700000000', '松原市');
INSERT INTO `j_position_city` VALUES ('60', '220', '220800000000', '白城市');
INSERT INTO `j_position_city` VALUES ('61', '220', '222400000000', '延边朝鲜族自治州');
INSERT INTO `j_position_city` VALUES ('62', '230', '230100000000', '哈尔滨市');
INSERT INTO `j_position_city` VALUES ('63', '230', '230200000000', '齐齐哈尔市');
INSERT INTO `j_position_city` VALUES ('64', '230', '230300000000', '鸡西市');
INSERT INTO `j_position_city` VALUES ('65', '230', '230400000000', '鹤岗市');
INSERT INTO `j_position_city` VALUES ('66', '230', '230500000000', '双鸭山市');
INSERT INTO `j_position_city` VALUES ('67', '230', '230600000000', '大庆市');
INSERT INTO `j_position_city` VALUES ('68', '230', '230700000000', '伊春市');
INSERT INTO `j_position_city` VALUES ('69', '230', '230800000000', '佳木斯市');
INSERT INTO `j_position_city` VALUES ('70', '230', '230900000000', '七台河市');
INSERT INTO `j_position_city` VALUES ('71', '230', '231000000000', '牡丹江市');
INSERT INTO `j_position_city` VALUES ('72', '230', '231100000000', '黑河市');
INSERT INTO `j_position_city` VALUES ('73', '230', '231200000000', '绥化市');
INSERT INTO `j_position_city` VALUES ('74', '230', '232700000000', '大兴安岭地区');
INSERT INTO `j_position_city` VALUES ('75', '310', '310100000000', '市辖区');
INSERT INTO `j_position_city` VALUES ('76', '310', '310200000000', '市辖县');
INSERT INTO `j_position_city` VALUES ('77', '320', '320100000000', '南京市');
INSERT INTO `j_position_city` VALUES ('78', '320', '320200000000', '无锡市');
INSERT INTO `j_position_city` VALUES ('79', '320', '320300000000', '徐州市');
INSERT INTO `j_position_city` VALUES ('80', '320', '320400000000', '常州市');
INSERT INTO `j_position_city` VALUES ('81', '320', '320500000000', '苏州市');
INSERT INTO `j_position_city` VALUES ('82', '320', '320600000000', '南通市');
INSERT INTO `j_position_city` VALUES ('83', '320', '320700000000', '连云港市');
INSERT INTO `j_position_city` VALUES ('84', '320', '320800000000', '淮安市');
INSERT INTO `j_position_city` VALUES ('85', '320', '320900000000', '盐城市');
INSERT INTO `j_position_city` VALUES ('86', '320', '321000000000', '扬州市');
INSERT INTO `j_position_city` VALUES ('87', '320', '321100000000', '镇江市');
INSERT INTO `j_position_city` VALUES ('88', '320', '321200000000', '泰州市');
INSERT INTO `j_position_city` VALUES ('89', '320', '321300000000', '宿迁市');
INSERT INTO `j_position_city` VALUES ('90', '330', '330100000000', '杭州市');
INSERT INTO `j_position_city` VALUES ('91', '330', '330200000000', '宁波市');
INSERT INTO `j_position_city` VALUES ('92', '330', '330300000000', '温州市');
INSERT INTO `j_position_city` VALUES ('93', '330', '330400000000', '嘉兴市');
INSERT INTO `j_position_city` VALUES ('94', '330', '330500000000', '湖州市');
INSERT INTO `j_position_city` VALUES ('95', '330', '330600000000', '绍兴市');
INSERT INTO `j_position_city` VALUES ('96', '330', '330700000000', '金华市');
INSERT INTO `j_position_city` VALUES ('97', '330', '330800000000', '衢州市');
INSERT INTO `j_position_city` VALUES ('98', '330', '330900000000', '舟山市');
INSERT INTO `j_position_city` VALUES ('99', '330', '331000000000', '台州市');
INSERT INTO `j_position_city` VALUES ('100', '330', '331100000000', '丽水市');
INSERT INTO `j_position_city` VALUES ('101', '340', '340100000000', '合肥市');
INSERT INTO `j_position_city` VALUES ('102', '340', '340200000000', '芜湖市');
INSERT INTO `j_position_city` VALUES ('103', '340', '340300000000', '蚌埠市');
INSERT INTO `j_position_city` VALUES ('104', '340', '340400000000', '淮南市');
INSERT INTO `j_position_city` VALUES ('105', '340', '340500000000', '马鞍山市');
INSERT INTO `j_position_city` VALUES ('106', '340', '340600000000', '淮北市');
INSERT INTO `j_position_city` VALUES ('107', '340', '340700000000', '铜陵市');
INSERT INTO `j_position_city` VALUES ('108', '340', '340800000000', '安庆市');
INSERT INTO `j_position_city` VALUES ('109', '340', '341000000000', '黄山市');
INSERT INTO `j_position_city` VALUES ('110', '340', '341100000000', '滁州市');
INSERT INTO `j_position_city` VALUES ('111', '340', '341200000000', '阜阳市');
INSERT INTO `j_position_city` VALUES ('112', '340', '341300000000', '宿州市');
INSERT INTO `j_position_city` VALUES ('113', '340', '341500000000', '六安市');
INSERT INTO `j_position_city` VALUES ('114', '340', '341600000000', '亳州市');
INSERT INTO `j_position_city` VALUES ('115', '340', '341700000000', '池州市');
INSERT INTO `j_position_city` VALUES ('116', '340', '341800000000', '宣城市');
INSERT INTO `j_position_city` VALUES ('117', '350', '350100000000', '福州市');
INSERT INTO `j_position_city` VALUES ('118', '350', '350200000000', '厦门市');
INSERT INTO `j_position_city` VALUES ('119', '350', '350300000000', '莆田市');
INSERT INTO `j_position_city` VALUES ('120', '350', '350400000000', '三明市');
INSERT INTO `j_position_city` VALUES ('121', '350', '350500000000', '泉州市');
INSERT INTO `j_position_city` VALUES ('122', '350', '350600000000', '漳州市');
INSERT INTO `j_position_city` VALUES ('123', '350', '350700000000', '南平市');
INSERT INTO `j_position_city` VALUES ('124', '350', '350800000000', '龙岩市');
INSERT INTO `j_position_city` VALUES ('125', '350', '350900000000', '宁德市');
INSERT INTO `j_position_city` VALUES ('126', '360', '360100000000', '南昌市');
INSERT INTO `j_position_city` VALUES ('127', '360', '360200000000', '景德镇市');
INSERT INTO `j_position_city` VALUES ('128', '360', '360300000000', '萍乡市');
INSERT INTO `j_position_city` VALUES ('129', '360', '360400000000', '九江市');
INSERT INTO `j_position_city` VALUES ('130', '360', '360500000000', '新余市');
INSERT INTO `j_position_city` VALUES ('131', '360', '360600000000', '鹰潭市');
INSERT INTO `j_position_city` VALUES ('132', '360', '360700000000', '赣州市');
INSERT INTO `j_position_city` VALUES ('133', '360', '360800000000', '吉安市');
INSERT INTO `j_position_city` VALUES ('134', '360', '360900000000', '宜春市');
INSERT INTO `j_position_city` VALUES ('135', '360', '361000000000', '抚州市');
INSERT INTO `j_position_city` VALUES ('136', '360', '361100000000', '上饶市');
INSERT INTO `j_position_city` VALUES ('137', '370', '370100000000', '济南市');
INSERT INTO `j_position_city` VALUES ('138', '370', '370200000000', '青岛市');
INSERT INTO `j_position_city` VALUES ('139', '370', '370300000000', '淄博市');
INSERT INTO `j_position_city` VALUES ('140', '370', '370400000000', '枣庄市');
INSERT INTO `j_position_city` VALUES ('141', '370', '370500000000', '东营市');
INSERT INTO `j_position_city` VALUES ('142', '370', '370600000000', '烟台市');
INSERT INTO `j_position_city` VALUES ('143', '370', '370700000000', '潍坊市');
INSERT INTO `j_position_city` VALUES ('144', '370', '370800000000', '济宁市');
INSERT INTO `j_position_city` VALUES ('145', '370', '370900000000', '泰安市');
INSERT INTO `j_position_city` VALUES ('146', '370', '371000000000', '威海市');
INSERT INTO `j_position_city` VALUES ('147', '370', '371100000000', '日照市');
INSERT INTO `j_position_city` VALUES ('148', '370', '371200000000', '莱芜市');
INSERT INTO `j_position_city` VALUES ('149', '370', '371300000000', '临沂市');
INSERT INTO `j_position_city` VALUES ('150', '370', '371400000000', '德州市');
INSERT INTO `j_position_city` VALUES ('151', '370', '371500000000', '聊城市');
INSERT INTO `j_position_city` VALUES ('152', '370', '371600000000', '滨州市');
INSERT INTO `j_position_city` VALUES ('153', '370', '371700000000', '菏泽市');
INSERT INTO `j_position_city` VALUES ('154', '410', '410100000000', '郑州市');
INSERT INTO `j_position_city` VALUES ('155', '410', '410200000000', '开封市');
INSERT INTO `j_position_city` VALUES ('156', '410', '410300000000', '洛阳市');
INSERT INTO `j_position_city` VALUES ('157', '410', '410400000000', '平顶山市');
INSERT INTO `j_position_city` VALUES ('158', '410', '410500000000', '安阳市');
INSERT INTO `j_position_city` VALUES ('159', '410', '410600000000', '鹤壁市');
INSERT INTO `j_position_city` VALUES ('160', '410', '410700000000', '新乡市');
INSERT INTO `j_position_city` VALUES ('161', '410', '410800000000', '焦作市');
INSERT INTO `j_position_city` VALUES ('162', '410', '410900000000', '濮阳市');
INSERT INTO `j_position_city` VALUES ('163', '410', '411000000000', '许昌市');
INSERT INTO `j_position_city` VALUES ('164', '410', '411100000000', '漯河市');
INSERT INTO `j_position_city` VALUES ('165', '410', '411200000000', '三门峡市');
INSERT INTO `j_position_city` VALUES ('166', '410', '411300000000', '南阳市');
INSERT INTO `j_position_city` VALUES ('167', '410', '411400000000', '商丘市');
INSERT INTO `j_position_city` VALUES ('168', '410', '411500000000', '信阳市');
INSERT INTO `j_position_city` VALUES ('169', '410', '411600000000', '周口市');
INSERT INTO `j_position_city` VALUES ('170', '410', '411700000000', '驻马店市');
INSERT INTO `j_position_city` VALUES ('171', '410', '419000000000', '省直辖县级行政区划');
INSERT INTO `j_position_city` VALUES ('172', '420', '420100000000', '武汉市');
INSERT INTO `j_position_city` VALUES ('173', '420', '420200000000', '黄石市');
INSERT INTO `j_position_city` VALUES ('174', '420', '420300000000', '十堰市');
INSERT INTO `j_position_city` VALUES ('175', '420', '420500000000', '宜昌市');
INSERT INTO `j_position_city` VALUES ('176', '420', '420600000000', '襄阳市');
INSERT INTO `j_position_city` VALUES ('177', '420', '420700000000', '鄂州市');
INSERT INTO `j_position_city` VALUES ('178', '420', '420800000000', '荆门市');
INSERT INTO `j_position_city` VALUES ('179', '420', '420900000000', '孝感市');
INSERT INTO `j_position_city` VALUES ('180', '420', '421000000000', '荆州市');
INSERT INTO `j_position_city` VALUES ('181', '420', '421100000000', '黄冈市');
INSERT INTO `j_position_city` VALUES ('182', '420', '421200000000', '咸宁市');
INSERT INTO `j_position_city` VALUES ('183', '420', '421300000000', '随州市');
INSERT INTO `j_position_city` VALUES ('184', '420', '422800000000', '恩施土家族苗族自治州');
INSERT INTO `j_position_city` VALUES ('185', '420', '429000000000', '省直辖县级行政区划');
INSERT INTO `j_position_city` VALUES ('186', '430', '430100000000', '长沙市');
INSERT INTO `j_position_city` VALUES ('187', '430', '430200000000', '株洲市');
INSERT INTO `j_position_city` VALUES ('188', '430', '430300000000', '湘潭市');
INSERT INTO `j_position_city` VALUES ('189', '430', '430400000000', '衡阳市');
INSERT INTO `j_position_city` VALUES ('190', '430', '430500000000', '邵阳市');
INSERT INTO `j_position_city` VALUES ('191', '430', '430600000000', '岳阳市');
INSERT INTO `j_position_city` VALUES ('192', '430', '430700000000', '常德市');
INSERT INTO `j_position_city` VALUES ('193', '430', '430800000000', '张家界市');
INSERT INTO `j_position_city` VALUES ('194', '430', '430900000000', '益阳市');
INSERT INTO `j_position_city` VALUES ('195', '430', '431000000000', '郴州市');
INSERT INTO `j_position_city` VALUES ('196', '430', '431100000000', '永州市');
INSERT INTO `j_position_city` VALUES ('197', '430', '431200000000', '怀化市');
INSERT INTO `j_position_city` VALUES ('198', '430', '431300000000', '娄底市');
INSERT INTO `j_position_city` VALUES ('199', '430', '433100000000', '湘西土家族苗族自治州');
INSERT INTO `j_position_city` VALUES ('200', '440', '440100000000', '广州市');
INSERT INTO `j_position_city` VALUES ('201', '440', '440200000000', '韶关市');
INSERT INTO `j_position_city` VALUES ('202', '440', '440300000000', '深圳市');
INSERT INTO `j_position_city` VALUES ('203', '440', '440400000000', '珠海市');
INSERT INTO `j_position_city` VALUES ('204', '440', '440500000000', '汕头市');
INSERT INTO `j_position_city` VALUES ('205', '440', '440600000000', '佛山市');
INSERT INTO `j_position_city` VALUES ('206', '440', '440700000000', '江门市');
INSERT INTO `j_position_city` VALUES ('207', '440', '440800000000', '湛江市');
INSERT INTO `j_position_city` VALUES ('208', '440', '440900000000', '茂名市');
INSERT INTO `j_position_city` VALUES ('209', '440', '441200000000', '肇庆市');
INSERT INTO `j_position_city` VALUES ('210', '440', '441300000000', '惠州市');
INSERT INTO `j_position_city` VALUES ('211', '440', '441400000000', '梅州市');
INSERT INTO `j_position_city` VALUES ('212', '440', '441500000000', '汕尾市');
INSERT INTO `j_position_city` VALUES ('213', '440', '441600000000', '河源市');
INSERT INTO `j_position_city` VALUES ('214', '440', '441700000000', '阳江市');
INSERT INTO `j_position_city` VALUES ('215', '440', '441800000000', '清远市');
INSERT INTO `j_position_city` VALUES ('216', '440', '441900000000', '东莞市');
INSERT INTO `j_position_city` VALUES ('217', '440', '442000000000', '中山市');
INSERT INTO `j_position_city` VALUES ('218', '440', '445100000000', '潮州市');
INSERT INTO `j_position_city` VALUES ('219', '440', '445200000000', '揭阳市');
INSERT INTO `j_position_city` VALUES ('220', '440', '445300000000', '云浮市');
INSERT INTO `j_position_city` VALUES ('221', '450', '450100000000', '南宁市');
INSERT INTO `j_position_city` VALUES ('222', '450', '450200000000', '柳州市');
INSERT INTO `j_position_city` VALUES ('223', '450', '450300000000', '桂林市');
INSERT INTO `j_position_city` VALUES ('224', '450', '450400000000', '梧州市');
INSERT INTO `j_position_city` VALUES ('225', '450', '450500000000', '北海市');
INSERT INTO `j_position_city` VALUES ('226', '450', '450600000000', '防城港市');
INSERT INTO `j_position_city` VALUES ('227', '450', '450700000000', '钦州市');
INSERT INTO `j_position_city` VALUES ('228', '450', '450800000000', '贵港市');
INSERT INTO `j_position_city` VALUES ('229', '450', '450900000000', '玉林市');
INSERT INTO `j_position_city` VALUES ('230', '450', '451000000000', '百色市');
INSERT INTO `j_position_city` VALUES ('231', '450', '451100000000', '贺州市');
INSERT INTO `j_position_city` VALUES ('232', '450', '451200000000', '河池市');
INSERT INTO `j_position_city` VALUES ('233', '450', '451300000000', '来宾市');
INSERT INTO `j_position_city` VALUES ('234', '450', '451400000000', '崇左市');
INSERT INTO `j_position_city` VALUES ('235', '460', '460100000000', '海口市');
INSERT INTO `j_position_city` VALUES ('236', '460', '460200000000', '三亚市');
INSERT INTO `j_position_city` VALUES ('237', '460', '460300000000', '三沙市');
INSERT INTO `j_position_city` VALUES ('238', '460', '469000000000', '省直辖县级行政区划');
INSERT INTO `j_position_city` VALUES ('239', '500', '500100000000', '市辖区');
INSERT INTO `j_position_city` VALUES ('240', '500', '500200000000', '市辖县');
INSERT INTO `j_position_city` VALUES ('241', '510', '510100000000', '成都市');
INSERT INTO `j_position_city` VALUES ('242', '510', '510300000000', '自贡市');
INSERT INTO `j_position_city` VALUES ('243', '510', '510400000000', '攀枝花市');
INSERT INTO `j_position_city` VALUES ('244', '510', '510500000000', '泸州市');
INSERT INTO `j_position_city` VALUES ('245', '510', '510600000000', '德阳市');
INSERT INTO `j_position_city` VALUES ('246', '510', '510700000000', '绵阳市');
INSERT INTO `j_position_city` VALUES ('247', '510', '510800000000', '广元市');
INSERT INTO `j_position_city` VALUES ('248', '510', '510900000000', '遂宁市');
INSERT INTO `j_position_city` VALUES ('249', '510', '511000000000', '内江市');
INSERT INTO `j_position_city` VALUES ('250', '510', '511100000000', '乐山市');
INSERT INTO `j_position_city` VALUES ('251', '510', '511300000000', '南充市');
INSERT INTO `j_position_city` VALUES ('252', '510', '511400000000', '眉山市');
INSERT INTO `j_position_city` VALUES ('253', '510', '511500000000', '宜宾市');
INSERT INTO `j_position_city` VALUES ('254', '510', '511600000000', '广安市');
INSERT INTO `j_position_city` VALUES ('255', '510', '511700000000', '达州市');
INSERT INTO `j_position_city` VALUES ('256', '510', '511800000000', '雅安市');
INSERT INTO `j_position_city` VALUES ('257', '510', '511900000000', '巴中市');
INSERT INTO `j_position_city` VALUES ('258', '510', '512000000000', '资阳市');
INSERT INTO `j_position_city` VALUES ('259', '510', '513200000000', '阿坝藏族羌族自治州');
INSERT INTO `j_position_city` VALUES ('260', '510', '513300000000', '甘孜藏族自治州');
INSERT INTO `j_position_city` VALUES ('261', '510', '513400000000', '凉山彝族自治州');
INSERT INTO `j_position_city` VALUES ('262', '520', '520100000000', '贵阳市');
INSERT INTO `j_position_city` VALUES ('263', '520', '520200000000', '六盘水市');
INSERT INTO `j_position_city` VALUES ('264', '520', '520300000000', '遵义市');
INSERT INTO `j_position_city` VALUES ('265', '520', '520400000000', '安顺市');
INSERT INTO `j_position_city` VALUES ('266', '520', '520500000000', '毕节市');
INSERT INTO `j_position_city` VALUES ('267', '520', '520600000000', '铜仁市');
INSERT INTO `j_position_city` VALUES ('268', '520', '522300000000', '黔西南布依族苗族自治州');
INSERT INTO `j_position_city` VALUES ('269', '520', '522600000000', '黔东南苗族侗族自治州');
INSERT INTO `j_position_city` VALUES ('270', '520', '522700000000', '黔南布依族苗族自治州');
INSERT INTO `j_position_city` VALUES ('271', '530', '530100000000', '昆明市');
INSERT INTO `j_position_city` VALUES ('272', '530', '530300000000', '曲靖市');
INSERT INTO `j_position_city` VALUES ('273', '530', '530400000000', '玉溪市');
INSERT INTO `j_position_city` VALUES ('274', '530', '530500000000', '保山市');
INSERT INTO `j_position_city` VALUES ('275', '530', '530600000000', '昭通市');
INSERT INTO `j_position_city` VALUES ('276', '530', '530700000000', '丽江市');
INSERT INTO `j_position_city` VALUES ('277', '530', '530800000000', '普洱市');
INSERT INTO `j_position_city` VALUES ('278', '530', '530900000000', '临沧市');
INSERT INTO `j_position_city` VALUES ('279', '530', '532300000000', '楚雄彝族自治州');
INSERT INTO `j_position_city` VALUES ('280', '530', '532500000000', '红河哈尼族彝族自治州');
INSERT INTO `j_position_city` VALUES ('281', '530', '532600000000', '文山壮族苗族自治州');
INSERT INTO `j_position_city` VALUES ('282', '530', '532800000000', '西双版纳傣族自治州');
INSERT INTO `j_position_city` VALUES ('283', '530', '532900000000', '大理白族自治州');
INSERT INTO `j_position_city` VALUES ('284', '530', '533100000000', '德宏傣族景颇族自治州');
INSERT INTO `j_position_city` VALUES ('285', '530', '533300000000', '怒江傈僳族自治州');
INSERT INTO `j_position_city` VALUES ('286', '530', '533400000000', '迪庆藏族自治州');
INSERT INTO `j_position_city` VALUES ('287', '540', '540100000000', '拉萨市');
INSERT INTO `j_position_city` VALUES ('288', '540', '542100000000', '昌都地区');
INSERT INTO `j_position_city` VALUES ('289', '540', '542200000000', '山南地区');
INSERT INTO `j_position_city` VALUES ('290', '540', '542300000000', '日喀则地区');
INSERT INTO `j_position_city` VALUES ('291', '540', '542400000000', '那曲地区');
INSERT INTO `j_position_city` VALUES ('292', '540', '542500000000', '阿里地区');
INSERT INTO `j_position_city` VALUES ('293', '540', '542600000000', '林芝地区');
INSERT INTO `j_position_city` VALUES ('294', '610', '610100000000', '西安市');
INSERT INTO `j_position_city` VALUES ('295', '610', '610200000000', '铜川市');
INSERT INTO `j_position_city` VALUES ('296', '610', '610300000000', '宝鸡市');
INSERT INTO `j_position_city` VALUES ('297', '610', '610400000000', '咸阳市');
INSERT INTO `j_position_city` VALUES ('298', '610', '610500000000', '渭南市');
INSERT INTO `j_position_city` VALUES ('299', '610', '610600000000', '延安市');
INSERT INTO `j_position_city` VALUES ('300', '610', '610700000000', '汉中市');
INSERT INTO `j_position_city` VALUES ('301', '610', '610800000000', '榆林市');
INSERT INTO `j_position_city` VALUES ('302', '610', '610900000000', '安康市');
INSERT INTO `j_position_city` VALUES ('303', '610', '611000000000', '商洛市');
INSERT INTO `j_position_city` VALUES ('304', '620', '620100000000', '兰州市');
INSERT INTO `j_position_city` VALUES ('305', '620', '620200000000', '嘉峪关市');
INSERT INTO `j_position_city` VALUES ('306', '620', '620300000000', '金昌市');
INSERT INTO `j_position_city` VALUES ('307', '620', '620400000000', '白银市');
INSERT INTO `j_position_city` VALUES ('308', '620', '620500000000', '天水市');
INSERT INTO `j_position_city` VALUES ('309', '620', '620600000000', '武威市');
INSERT INTO `j_position_city` VALUES ('310', '620', '620700000000', '张掖市');
INSERT INTO `j_position_city` VALUES ('311', '620', '620800000000', '平凉市');
INSERT INTO `j_position_city` VALUES ('312', '620', '620900000000', '酒泉市');
INSERT INTO `j_position_city` VALUES ('313', '620', '621000000000', '庆阳市');
INSERT INTO `j_position_city` VALUES ('314', '620', '621100000000', '定西市');
INSERT INTO `j_position_city` VALUES ('315', '620', '621200000000', '陇南市');
INSERT INTO `j_position_city` VALUES ('316', '620', '622900000000', '临夏回族自治州');
INSERT INTO `j_position_city` VALUES ('317', '620', '623000000000', '甘南藏族自治州');
INSERT INTO `j_position_city` VALUES ('318', '630', '630100000000', '西宁市');
INSERT INTO `j_position_city` VALUES ('319', '630', '630200000000', '海东市');
INSERT INTO `j_position_city` VALUES ('320', '630', '632200000000', '海北藏族自治州');
INSERT INTO `j_position_city` VALUES ('321', '630', '632300000000', '黄南藏族自治州');
INSERT INTO `j_position_city` VALUES ('322', '630', '632500000000', '海南藏族自治州');
INSERT INTO `j_position_city` VALUES ('323', '630', '632600000000', '果洛藏族自治州');
INSERT INTO `j_position_city` VALUES ('324', '630', '632700000000', '玉树藏族自治州');
INSERT INTO `j_position_city` VALUES ('325', '630', '632800000000', '海西蒙古族藏族自治州');
INSERT INTO `j_position_city` VALUES ('326', '640', '640100000000', '银川市');
INSERT INTO `j_position_city` VALUES ('327', '640', '640200000000', '石嘴山市');
INSERT INTO `j_position_city` VALUES ('328', '640', '640300000000', '吴忠市');
INSERT INTO `j_position_city` VALUES ('329', '640', '640400000000', '固原市');
INSERT INTO `j_position_city` VALUES ('330', '640', '640500000000', '中卫市');
INSERT INTO `j_position_city` VALUES ('331', '650', '650100000000', '乌鲁木齐市');
INSERT INTO `j_position_city` VALUES ('332', '650', '650200000000', '克拉玛依市');
INSERT INTO `j_position_city` VALUES ('333', '650', '652100000000', '吐鲁番地区');
INSERT INTO `j_position_city` VALUES ('334', '650', '652200000000', '哈密地区');
INSERT INTO `j_position_city` VALUES ('335', '650', '652300000000', '昌吉回族自治州');
INSERT INTO `j_position_city` VALUES ('336', '650', '652700000000', '博尔塔拉蒙古自治州');
INSERT INTO `j_position_city` VALUES ('337', '650', '652800000000', '巴音郭楞蒙古自治州');
INSERT INTO `j_position_city` VALUES ('338', '650', '652900000000', '阿克苏地区');
INSERT INTO `j_position_city` VALUES ('339', '650', '653000000000', '克孜勒苏柯尔克孜自治州');
INSERT INTO `j_position_city` VALUES ('340', '650', '653100000000', '喀什地区');
INSERT INTO `j_position_city` VALUES ('341', '650', '653200000000', '和田地区');
INSERT INTO `j_position_city` VALUES ('342', '650', '654000000000', '伊犁哈萨克自治州');
INSERT INTO `j_position_city` VALUES ('343', '650', '654200000000', '塔城地区');
INSERT INTO `j_position_city` VALUES ('344', '650', '654300000000', '阿勒泰地区');
INSERT INTO `j_position_city` VALUES ('345', '650', '659000000000', '自治区直辖县级行政区划');

*/


/* === (20) j_position_county === */
SELECT '-----j_position_county prcess--------';


DROP TABLE IF EXISTS `j_position_county`;
CREATE TABLE `j_position_county` 
(
  `id` int(11) NOT NULL AUTO_INCREMENT COMMENT '地级市主键ID',
  `city_id` bigint(20) unsigned NOT NULL COMMENT '地级市id',
  `county_id` bigint(20) unsigned NOT NULL COMMENT '县级id',
  `county_name` char(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `county_id` (`county_id`),
  KEY `city_id` (`city_id`)
  
) ENGINE=InnoDB AUTO_INCREMENT=2857 DEFAULT CHARSET=utf8 COMMENT='地区市数据库';


/*

INSERT INTO `j_position_county` VALUES ('1',  '110100000000', '110101000000', '东城区');
INSERT INTO `j_position_county` VALUES ('2',  '110100000000', '110102000000', '西城区');
INSERT INTO `j_position_county` VALUES ('3',  '110100000000', '110105000000', '朝阳区');
INSERT INTO `j_position_county` VALUES ('4',  '110100000000', '110106000000', '丰台区');
INSERT INTO `j_position_county` VALUES ('5',  '110100000000', '110107000000', '石景山区');
INSERT INTO `j_position_county` VALUES ('6',  '110100000000', '110108000000', '海淀区');
INSERT INTO `j_position_county` VALUES ('7',  '110100000000', '110109000000', '门头沟区');
INSERT INTO `j_position_county` VALUES ('8',  '110100000000', '110111000000', '房山区');
INSERT INTO `j_position_county` VALUES ('9',  '110100000000', '110112000000', '通州区');
INSERT INTO `j_position_county` VALUES ('10', '110100000000', '110113000000', '顺义区');
INSERT INTO `j_position_county` VALUES ('11', '110100000000', '110114000000', '昌平区');
INSERT INTO `j_position_county` VALUES ('12', '110100000000', '110115000000', '大兴区');
INSERT INTO `j_position_county` VALUES ('13', '110100000000', '110116000000', '怀柔区');
INSERT INTO `j_position_county` VALUES ('14', '110100000000', '110117000000', '平谷区');
INSERT INTO `j_position_county` VALUES ('15', '110200000000', '110228000000', '密云县');
INSERT INTO `j_position_county` VALUES ('16', '110200000000', '110229000000', '延庆县');
INSERT INTO `j_position_county` VALUES ('17', '120100000000', '120101000000', '和平区');
INSERT INTO `j_position_county` VALUES ('18', '120100000000', '120102000000', '河东区');
INSERT INTO `j_position_county` VALUES ('19', '120100000000', '120103000000', '河西区');
INSERT INTO `j_position_county` VALUES ('20', '120100000000', '120104000000', '南开区');
INSERT INTO `j_position_county` VALUES ('21', '120100000000', '120105000000', '河北区');
INSERT INTO `j_position_county` VALUES ('22', '120100000000', '120106000000', '红桥区');
INSERT INTO `j_position_county` VALUES ('23', '120100000000', '120110000000', '东丽区');
INSERT INTO `j_position_county` VALUES ('24', '120100000000', '120111000000', '西青区');
INSERT INTO `j_position_county` VALUES ('25', '120100000000', '120112000000', '津南区');
INSERT INTO `j_position_county` VALUES ('26', '120100000000', '120113000000', '北辰区');
INSERT INTO `j_position_county` VALUES ('27', '120100000000', '120114000000', '武清区');
INSERT INTO `j_position_county` VALUES ('28', '120100000000', '120115000000', '宝坻区');
INSERT INTO `j_position_county` VALUES ('29', '120100000000', '120116000000', '滨海新区');
INSERT INTO `j_position_county` VALUES ('30', '120200000000', '120221000000', '宁河县');
INSERT INTO `j_position_county` VALUES ('31', '120200000000', '120223000000', '静海县');
INSERT INTO `j_position_county` VALUES ('32', '120200000000', '120225000000', '蓟县');
INSERT INTO `j_position_county` VALUES ('33', '130100000000', '130102000000', '长安区');
INSERT INTO `j_position_county` VALUES ('34', '130100000000', '130103000000', '桥东区');
INSERT INTO `j_position_county` VALUES ('35', '130100000000', '130104000000', '桥西区');
INSERT INTO `j_position_county` VALUES ('36', '130100000000', '130105000000', '新华区');
INSERT INTO `j_position_county` VALUES ('37', '130100000000', '130107000000', '井陉矿区');
INSERT INTO `j_position_county` VALUES ('38', '130100000000', '130108000000', '裕华区');
INSERT INTO `j_position_county` VALUES ('39', '130100000000', '130121000000', '井陉县');
INSERT INTO `j_position_county` VALUES ('40', '130100000000', '130123000000', '正定县');
INSERT INTO `j_position_county` VALUES ('41', '130100000000', '130124000000', '栾城县');
INSERT INTO `j_position_county` VALUES ('42', '130100000000', '130125000000', '行唐县');
INSERT INTO `j_position_county` VALUES ('43', '130100000000', '130126000000', '灵寿县');
INSERT INTO `j_position_county` VALUES ('44', '130100000000', '130127000000', '高邑县');
INSERT INTO `j_position_county` VALUES ('45', '130100000000', '130128000000', '深泽县');
INSERT INTO `j_position_county` VALUES ('46', '130100000000', '130129000000', '赞皇县');
INSERT INTO `j_position_county` VALUES ('47', '130100000000', '130130000000', '无极县');
INSERT INTO `j_position_county` VALUES ('48', '130100000000', '130131000000', '平山县');
INSERT INTO `j_position_county` VALUES ('49', '130100000000', '130132000000', '元氏县');
INSERT INTO `j_position_county` VALUES ('50', '130100000000', '130133000000', '赵县');
INSERT INTO `j_position_county` VALUES ('51', '130100000000', '130181000000', '辛集市');
INSERT INTO `j_position_county` VALUES ('52', '130100000000', '130182000000', '藁城市');
INSERT INTO `j_position_county` VALUES ('53', '130100000000', '130183000000', '晋州市');
INSERT INTO `j_position_county` VALUES ('54', '130100000000', '130184000000', '新乐市');
INSERT INTO `j_position_county` VALUES ('55', '130100000000', '130185000000', '鹿泉市');
INSERT INTO `j_position_county` VALUES ('56', '130200000000', '130202000000', '路南区');
INSERT INTO `j_position_county` VALUES ('57', '130200000000', '130203000000', '路北区');
INSERT INTO `j_position_county` VALUES ('58', '130200000000', '130204000000', '古冶区');
INSERT INTO `j_position_county` VALUES ('59', '130200000000', '130205000000', '开平区');
INSERT INTO `j_position_county` VALUES ('60', '130200000000', '130207000000', '丰南区');
INSERT INTO `j_position_county` VALUES ('61', '130200000000', '130208000000', '丰润区');
INSERT INTO `j_position_county` VALUES ('62', '130200000000', '130209000000', '曹妃甸区');
INSERT INTO `j_position_county` VALUES ('63', '130200000000', '130223000000', '滦县');
INSERT INTO `j_position_county` VALUES ('64', '130200000000', '130224000000', '滦南县');
INSERT INTO `j_position_county` VALUES ('65', '130200000000', '130225000000', '乐亭县');
INSERT INTO `j_position_county` VALUES ('66', '130200000000', '130227000000', '迁西县');
INSERT INTO `j_position_county` VALUES ('67', '130200000000', '130229000000', '玉田县');
INSERT INTO `j_position_county` VALUES ('68', '130200000000', '130281000000', '遵化市');
INSERT INTO `j_position_county` VALUES ('69', '130200000000', '130283000000', '迁安市');
INSERT INTO `j_position_county` VALUES ('70', '130300000000', '130302000000', '海港区');
INSERT INTO `j_position_county` VALUES ('71', '130300000000', '130303000000', '山海关区');
INSERT INTO `j_position_county` VALUES ('72', '130300000000', '130304000000', '北戴河区');
INSERT INTO `j_position_county` VALUES ('73', '130300000000', '130321000000', '青龙满族自治县');
INSERT INTO `j_position_county` VALUES ('74', '130300000000', '130322000000', '昌黎县');
INSERT INTO `j_position_county` VALUES ('75', '130300000000', '130323000000', '抚宁县');
INSERT INTO `j_position_county` VALUES ('76', '130300000000', '130324000000', '卢龙县');
INSERT INTO `j_position_county` VALUES ('77', '130400000000', '130402000000', '邯山区');
INSERT INTO `j_position_county` VALUES ('78', '130400000000', '130403000000', '丛台区');
INSERT INTO `j_position_county` VALUES ('79', '130400000000', '130404000000', '复兴区');
INSERT INTO `j_position_county` VALUES ('80', '130400000000', '130406000000', '峰峰矿区');
INSERT INTO `j_position_county` VALUES ('81', '130400000000', '130421000000', '邯郸县');
INSERT INTO `j_position_county` VALUES ('82', '130400000000', '130423000000', '临漳县');
INSERT INTO `j_position_county` VALUES ('83', '130400000000', '130424000000', '成安县');
INSERT INTO `j_position_county` VALUES ('84', '130400000000', '130425000000', '大名县');
INSERT INTO `j_position_county` VALUES ('85', '130400000000', '130426000000', '涉县');
INSERT INTO `j_position_county` VALUES ('86', '130400000000', '130427000000', '磁县');
INSERT INTO `j_position_county` VALUES ('87', '130400000000', '130428000000', '肥乡县');
INSERT INTO `j_position_county` VALUES ('88', '130400000000', '130429000000', '永年县');
INSERT INTO `j_position_county` VALUES ('89', '130400000000', '130430000000', '邱县');
INSERT INTO `j_position_county` VALUES ('90', '130400000000', '130431000000', '鸡泽县');
INSERT INTO `j_position_county` VALUES ('91', '130400000000', '130432000000', '广平县');
INSERT INTO `j_position_county` VALUES ('92', '130400000000', '130433000000', '馆陶县');
INSERT INTO `j_position_county` VALUES ('93', '130400000000', '130434000000', '魏县');
INSERT INTO `j_position_county` VALUES ('94', '130400000000', '130435000000', '曲周县');
INSERT INTO `j_position_county` VALUES ('95', '130400000000', '130481000000', '武安市');
INSERT INTO `j_position_county` VALUES ('96', '130500000000', '130502000000', '桥东区');
INSERT INTO `j_position_county` VALUES ('97', '130500000000', '130503000000', '桥西区');
INSERT INTO `j_position_county` VALUES ('98', '130500000000', '130521000000', '邢台县');
INSERT INTO `j_position_county` VALUES ('99', '130500000000', '130522000000', '临城县');
INSERT INTO `j_position_county` VALUES ('100', '130500000000', '130523000000', '内丘县');
INSERT INTO `j_position_county` VALUES ('101', '130500000000', '130524000000', '柏乡县');
INSERT INTO `j_position_county` VALUES ('102', '130500000000', '130525000000', '隆尧县');
INSERT INTO `j_position_county` VALUES ('103', '130500000000', '130526000000', '任县');
INSERT INTO `j_position_county` VALUES ('104', '130500000000', '130527000000', '南和县');
INSERT INTO `j_position_county` VALUES ('105', '130500000000', '130528000000', '宁晋县');
INSERT INTO `j_position_county` VALUES ('106', '130500000000', '130529000000', '巨鹿县');
INSERT INTO `j_position_county` VALUES ('107', '130500000000', '130530000000', '新河县');
INSERT INTO `j_position_county` VALUES ('108', '130500000000', '130531000000', '广宗县');
INSERT INTO `j_position_county` VALUES ('109', '130500000000', '130532000000', '平乡县');
INSERT INTO `j_position_county` VALUES ('110', '130500000000', '130533000000', '威县');
INSERT INTO `j_position_county` VALUES ('111', '130500000000', '130534000000', '清河县');
INSERT INTO `j_position_county` VALUES ('112', '130500000000', '130535000000', '临西县');
INSERT INTO `j_position_county` VALUES ('113', '130500000000', '130581000000', '南宫市');
INSERT INTO `j_position_county` VALUES ('114', '130500000000', '130582000000', '沙河市');
INSERT INTO `j_position_county` VALUES ('115', '130600000000', '130602000000', '新市区');
INSERT INTO `j_position_county` VALUES ('116', '130600000000', '130603000000', '北市区');
INSERT INTO `j_position_county` VALUES ('117', '130600000000', '130604000000', '南市区');
INSERT INTO `j_position_county` VALUES ('118', '130600000000', '130621000000', '满城县');
INSERT INTO `j_position_county` VALUES ('119', '130600000000', '130622000000', '清苑县');
INSERT INTO `j_position_county` VALUES ('120', '130600000000', '130623000000', '涞水县');
INSERT INTO `j_position_county` VALUES ('121', '130600000000', '130624000000', '阜平县');
INSERT INTO `j_position_county` VALUES ('122', '130600000000', '130625000000', '徐水县');
INSERT INTO `j_position_county` VALUES ('123', '130600000000', '130626000000', '定兴县');
INSERT INTO `j_position_county` VALUES ('124', '130600000000', '130627000000', '唐县');
INSERT INTO `j_position_county` VALUES ('125', '130600000000', '130628000000', '高阳县');
INSERT INTO `j_position_county` VALUES ('126', '130600000000', '130629000000', '容城县');
INSERT INTO `j_position_county` VALUES ('127', '130600000000', '130630000000', '涞源县');
INSERT INTO `j_position_county` VALUES ('128', '130600000000', '130631000000', '望都县');
INSERT INTO `j_position_county` VALUES ('129', '130600000000', '130632000000', '安新县');
INSERT INTO `j_position_county` VALUES ('130', '130600000000', '130633000000', '易县');
INSERT INTO `j_position_county` VALUES ('131', '130600000000', '130634000000', '曲阳县');
INSERT INTO `j_position_county` VALUES ('132', '130600000000', '130635000000', '蠡县');
INSERT INTO `j_position_county` VALUES ('133', '130600000000', '130636000000', '顺平县');
INSERT INTO `j_position_county` VALUES ('134', '130600000000', '130637000000', '博野县');
INSERT INTO `j_position_county` VALUES ('135', '130600000000', '130638000000', '雄县');
INSERT INTO `j_position_county` VALUES ('136', '130600000000', '130681000000', '涿州市');
INSERT INTO `j_position_county` VALUES ('137', '130600000000', '130682000000', '定州市');
INSERT INTO `j_position_county` VALUES ('138', '130600000000', '130683000000', '安国市');
INSERT INTO `j_position_county` VALUES ('139', '130600000000', '130684000000', '高碑店市');
INSERT INTO `j_position_county` VALUES ('140', '130700000000', '130702000000', '桥东区');
INSERT INTO `j_position_county` VALUES ('141', '130700000000', '130703000000', '桥西区');
INSERT INTO `j_position_county` VALUES ('142', '130700000000', '130705000000', '宣化区');
INSERT INTO `j_position_county` VALUES ('143', '130700000000', '130706000000', '下花园区');
INSERT INTO `j_position_county` VALUES ('144', '130700000000', '130721000000', '宣化县');
INSERT INTO `j_position_county` VALUES ('145', '130700000000', '130722000000', '张北县');
INSERT INTO `j_position_county` VALUES ('146', '130700000000', '130723000000', '康保县');
INSERT INTO `j_position_county` VALUES ('147', '130700000000', '130724000000', '沽源县');
INSERT INTO `j_position_county` VALUES ('148', '130700000000', '130725000000', '尚义县');
INSERT INTO `j_position_county` VALUES ('149', '130700000000', '130726000000', '蔚县');
INSERT INTO `j_position_county` VALUES ('150', '130700000000', '130727000000', '阳原县');
INSERT INTO `j_position_county` VALUES ('151', '130700000000', '130728000000', '怀安县');
INSERT INTO `j_position_county` VALUES ('152', '130700000000', '130729000000', '万全县');
INSERT INTO `j_position_county` VALUES ('153', '130700000000', '130730000000', '怀来县');
INSERT INTO `j_position_county` VALUES ('154', '130700000000', '130731000000', '涿鹿县');
INSERT INTO `j_position_county` VALUES ('155', '130700000000', '130732000000', '赤城县');
INSERT INTO `j_position_county` VALUES ('156', '130700000000', '130733000000', '崇礼县');
INSERT INTO `j_position_county` VALUES ('157', '130800000000', '130802000000', '双桥区');
INSERT INTO `j_position_county` VALUES ('158', '130800000000', '130803000000', '双滦区');
INSERT INTO `j_position_county` VALUES ('159', '130800000000', '130804000000', '鹰手营子矿区');
INSERT INTO `j_position_county` VALUES ('160', '130800000000', '130821000000', '承德县');
INSERT INTO `j_position_county` VALUES ('161', '130800000000', '130822000000', '兴隆县');
INSERT INTO `j_position_county` VALUES ('162', '130800000000', '130823000000', '平泉县');
INSERT INTO `j_position_county` VALUES ('163', '130800000000', '130824000000', '滦平县');
INSERT INTO `j_position_county` VALUES ('164', '130800000000', '130825000000', '隆化县');
INSERT INTO `j_position_county` VALUES ('165', '130800000000', '130826000000', '丰宁满族自治县');
INSERT INTO `j_position_county` VALUES ('166', '130800000000', '130827000000', '宽城满族自治县');
INSERT INTO `j_position_county` VALUES ('167', '130800000000', '130828000000', '围场满族蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('168', '130900000000', '130902000000', '新华区');
INSERT INTO `j_position_county` VALUES ('169', '130900000000', '130903000000', '运河区');
INSERT INTO `j_position_county` VALUES ('170', '130900000000', '130921000000', '沧县');
INSERT INTO `j_position_county` VALUES ('171', '130900000000', '130922000000', '青县');
INSERT INTO `j_position_county` VALUES ('172', '130900000000', '130923000000', '东光县');
INSERT INTO `j_position_county` VALUES ('173', '130900000000', '130924000000', '海兴县');
INSERT INTO `j_position_county` VALUES ('174', '130900000000', '130925000000', '盐山县');
INSERT INTO `j_position_county` VALUES ('175', '130900000000', '130926000000', '肃宁县');
INSERT INTO `j_position_county` VALUES ('176', '130900000000', '130927000000', '南皮县');
INSERT INTO `j_position_county` VALUES ('177', '130900000000', '130928000000', '吴桥县');
INSERT INTO `j_position_county` VALUES ('178', '130900000000', '130929000000', '献县');
INSERT INTO `j_position_county` VALUES ('179', '130900000000', '130930000000', '孟村回族自治县');
INSERT INTO `j_position_county` VALUES ('180', '130900000000', '130981000000', '泊头市');
INSERT INTO `j_position_county` VALUES ('181', '130900000000', '130982000000', '任丘市');
INSERT INTO `j_position_county` VALUES ('182', '130900000000', '130983000000', '黄骅市');
INSERT INTO `j_position_county` VALUES ('183', '130900000000', '130984000000', '河间市');
INSERT INTO `j_position_county` VALUES ('184', '131000000000', '131002000000', '安次区');
INSERT INTO `j_position_county` VALUES ('185', '131000000000', '131003000000', '广阳区');
INSERT INTO `j_position_county` VALUES ('186', '131000000000', '131022000000', '固安县');
INSERT INTO `j_position_county` VALUES ('187', '131000000000', '131023000000', '永清县');
INSERT INTO `j_position_county` VALUES ('188', '131000000000', '131024000000', '香河县');
INSERT INTO `j_position_county` VALUES ('189', '131000000000', '131025000000', '大城县');
INSERT INTO `j_position_county` VALUES ('190', '131000000000', '131026000000', '文安县');
INSERT INTO `j_position_county` VALUES ('191', '131000000000', '131028000000', '大厂回族自治县');
INSERT INTO `j_position_county` VALUES ('192', '131000000000', '131081000000', '霸州市');
INSERT INTO `j_position_county` VALUES ('193', '131000000000', '131082000000', '三河市');
INSERT INTO `j_position_county` VALUES ('194', '131100000000', '131102000000', '桃城区');
INSERT INTO `j_position_county` VALUES ('195', '131100000000', '131121000000', '枣强县');
INSERT INTO `j_position_county` VALUES ('196', '131100000000', '131122000000', '武邑县');
INSERT INTO `j_position_county` VALUES ('197', '131100000000', '131123000000', '武强县');
INSERT INTO `j_position_county` VALUES ('198', '131100000000', '131124000000', '饶阳县');
INSERT INTO `j_position_county` VALUES ('199', '131100000000', '131125000000', '安平县');
INSERT INTO `j_position_county` VALUES ('200', '131100000000', '131126000000', '故城县');
INSERT INTO `j_position_county` VALUES ('201', '131100000000', '131127000000', '景县');
INSERT INTO `j_position_county` VALUES ('202', '131100000000', '131128000000', '阜城县');
INSERT INTO `j_position_county` VALUES ('203', '131100000000', '131181000000', '冀州市');
INSERT INTO `j_position_county` VALUES ('204', '131100000000', '131182000000', '深州市');
INSERT INTO `j_position_county` VALUES ('205', '140100000000', '140105000000', '小店区');
INSERT INTO `j_position_county` VALUES ('206', '140100000000', '140106000000', '迎泽区');
INSERT INTO `j_position_county` VALUES ('207', '140100000000', '140107000000', '杏花岭区');
INSERT INTO `j_position_county` VALUES ('208', '140100000000', '140108000000', '尖草坪区');
INSERT INTO `j_position_county` VALUES ('209', '140100000000', '140109000000', '万柏林区');
INSERT INTO `j_position_county` VALUES ('210', '140100000000', '140110000000', '晋源区');
INSERT INTO `j_position_county` VALUES ('211', '140100000000', '140121000000', '清徐县');
INSERT INTO `j_position_county` VALUES ('212', '140100000000', '140122000000', '阳曲县');
INSERT INTO `j_position_county` VALUES ('213', '140100000000', '140123000000', '娄烦县');
INSERT INTO `j_position_county` VALUES ('214', '140100000000', '140181000000', '古交市');
INSERT INTO `j_position_county` VALUES ('215', '140200000000', '140202000000', '城区');
INSERT INTO `j_position_county` VALUES ('216', '140200000000', '140203000000', '矿区');
INSERT INTO `j_position_county` VALUES ('217', '140200000000', '140211000000', '南郊区');
INSERT INTO `j_position_county` VALUES ('218', '140200000000', '140212000000', '新荣区');
INSERT INTO `j_position_county` VALUES ('219', '140200000000', '140221000000', '阳高县');
INSERT INTO `j_position_county` VALUES ('220', '140200000000', '140222000000', '天镇县');
INSERT INTO `j_position_county` VALUES ('221', '140200000000', '140223000000', '广灵县');
INSERT INTO `j_position_county` VALUES ('222', '140200000000', '140224000000', '灵丘县');
INSERT INTO `j_position_county` VALUES ('223', '140200000000', '140225000000', '浑源县');
INSERT INTO `j_position_county` VALUES ('224', '140200000000', '140226000000', '左云县');
INSERT INTO `j_position_county` VALUES ('225', '140200000000', '140227000000', '大同县');
INSERT INTO `j_position_county` VALUES ('226', '140300000000', '140302000000', '城区');
INSERT INTO `j_position_county` VALUES ('227', '140300000000', '140303000000', '矿区');
INSERT INTO `j_position_county` VALUES ('228', '140300000000', '140311000000', '郊区');
INSERT INTO `j_position_county` VALUES ('229', '140300000000', '140321000000', '平定县');
INSERT INTO `j_position_county` VALUES ('230', '140300000000', '140322000000', '盂县');
INSERT INTO `j_position_county` VALUES ('231', '140400000000', '140402000000', '城区');
INSERT INTO `j_position_county` VALUES ('232', '140400000000', '140411000000', '郊区');
INSERT INTO `j_position_county` VALUES ('233', '140400000000', '140421000000', '长治县');
INSERT INTO `j_position_county` VALUES ('234', '140400000000', '140423000000', '襄垣县');
INSERT INTO `j_position_county` VALUES ('235', '140400000000', '140424000000', '屯留县');
INSERT INTO `j_position_county` VALUES ('236', '140400000000', '140425000000', '平顺县');
INSERT INTO `j_position_county` VALUES ('237', '140400000000', '140426000000', '黎城县');
INSERT INTO `j_position_county` VALUES ('238', '140400000000', '140427000000', '壶关县');
INSERT INTO `j_position_county` VALUES ('239', '140400000000', '140428000000', '长子县');
INSERT INTO `j_position_county` VALUES ('240', '140400000000', '140429000000', '武乡县');
INSERT INTO `j_position_county` VALUES ('241', '140400000000', '140430000000', '沁县');
INSERT INTO `j_position_county` VALUES ('242', '140400000000', '140431000000', '沁源县');
INSERT INTO `j_position_county` VALUES ('243', '140400000000', '140481000000', '潞城市');
INSERT INTO `j_position_county` VALUES ('244', '140500000000', '140502000000', '城区');
INSERT INTO `j_position_county` VALUES ('245', '140500000000', '140521000000', '沁水县');
INSERT INTO `j_position_county` VALUES ('246', '140500000000', '140522000000', '阳城县');
INSERT INTO `j_position_county` VALUES ('247', '140500000000', '140524000000', '陵川县');
INSERT INTO `j_position_county` VALUES ('248', '140500000000', '140525000000', '泽州县');
INSERT INTO `j_position_county` VALUES ('249', '140500000000', '140581000000', '高平市');
INSERT INTO `j_position_county` VALUES ('250', '140600000000', '140602000000', '朔城区');
INSERT INTO `j_position_county` VALUES ('251', '140600000000', '140603000000', '平鲁区');
INSERT INTO `j_position_county` VALUES ('252', '140600000000', '140621000000', '山阴县');
INSERT INTO `j_position_county` VALUES ('253', '140600000000', '140622000000', '应县');
INSERT INTO `j_position_county` VALUES ('254', '140600000000', '140623000000', '右玉县');
INSERT INTO `j_position_county` VALUES ('255', '140600000000', '140624000000', '怀仁县');
INSERT INTO `j_position_county` VALUES ('256', '140700000000', '140702000000', '榆次区');
INSERT INTO `j_position_county` VALUES ('257', '140700000000', '140721000000', '榆社县');
INSERT INTO `j_position_county` VALUES ('258', '140700000000', '140722000000', '左权县');
INSERT INTO `j_position_county` VALUES ('259', '140700000000', '140723000000', '和顺县');
INSERT INTO `j_position_county` VALUES ('260', '140700000000', '140724000000', '昔阳县');
INSERT INTO `j_position_county` VALUES ('261', '140700000000', '140725000000', '寿阳县');
INSERT INTO `j_position_county` VALUES ('262', '140700000000', '140726000000', '太谷县');
INSERT INTO `j_position_county` VALUES ('263', '140700000000', '140727000000', '祁县');
INSERT INTO `j_position_county` VALUES ('264', '140700000000', '140728000000', '平遥县');
INSERT INTO `j_position_county` VALUES ('265', '140700000000', '140729000000', '灵石县');
INSERT INTO `j_position_county` VALUES ('266', '140700000000', '140781000000', '介休市');
INSERT INTO `j_position_county` VALUES ('267', '140800000000', '140802000000', '盐湖区');
INSERT INTO `j_position_county` VALUES ('268', '140800000000', '140821000000', '临猗县');
INSERT INTO `j_position_county` VALUES ('269', '140800000000', '140822000000', '万荣县');
INSERT INTO `j_position_county` VALUES ('270', '140800000000', '140823000000', '闻喜县');
INSERT INTO `j_position_county` VALUES ('271', '140800000000', '140824000000', '稷山县');
INSERT INTO `j_position_county` VALUES ('272', '140800000000', '140825000000', '新绛县');
INSERT INTO `j_position_county` VALUES ('273', '140800000000', '140826000000', '绛县');
INSERT INTO `j_position_county` VALUES ('274', '140800000000', '140827000000', '垣曲县');
INSERT INTO `j_position_county` VALUES ('275', '140800000000', '140828000000', '夏县');
INSERT INTO `j_position_county` VALUES ('276', '140800000000', '140829000000', '平陆县');
INSERT INTO `j_position_county` VALUES ('277', '140800000000', '140830000000', '芮城县');
INSERT INTO `j_position_county` VALUES ('278', '140800000000', '140881000000', '永济市');
INSERT INTO `j_position_county` VALUES ('279', '140800000000', '140882000000', '河津市');
INSERT INTO `j_position_county` VALUES ('280', '140900000000', '140902000000', '忻府区');
INSERT INTO `j_position_county` VALUES ('281', '140900000000', '140921000000', '定襄县');
INSERT INTO `j_position_county` VALUES ('282', '140900000000', '140922000000', '五台县');
INSERT INTO `j_position_county` VALUES ('283', '140900000000', '140923000000', '代县');
INSERT INTO `j_position_county` VALUES ('284', '140900000000', '140924000000', '繁峙县');
INSERT INTO `j_position_county` VALUES ('285', '140900000000', '140925000000', '宁武县');
INSERT INTO `j_position_county` VALUES ('286', '140900000000', '140926000000', '静乐县');
INSERT INTO `j_position_county` VALUES ('287', '140900000000', '140927000000', '神池县');
INSERT INTO `j_position_county` VALUES ('288', '140900000000', '140928000000', '五寨县');
INSERT INTO `j_position_county` VALUES ('289', '140900000000', '140929000000', '岢岚县');
INSERT INTO `j_position_county` VALUES ('290', '140900000000', '140930000000', '河曲县');
INSERT INTO `j_position_county` VALUES ('291', '140900000000', '140931000000', '保德县');
INSERT INTO `j_position_county` VALUES ('292', '140900000000', '140932000000', '偏关县');
INSERT INTO `j_position_county` VALUES ('293', '140900000000', '140981000000', '原平市');
INSERT INTO `j_position_county` VALUES ('294', '141000000000', '141002000000', '尧都区');
INSERT INTO `j_position_county` VALUES ('295', '141000000000', '141021000000', '曲沃县');
INSERT INTO `j_position_county` VALUES ('296', '141000000000', '141022000000', '翼城县');
INSERT INTO `j_position_county` VALUES ('297', '141000000000', '141023000000', '襄汾县');
INSERT INTO `j_position_county` VALUES ('298', '141000000000', '141024000000', '洪洞县');
INSERT INTO `j_position_county` VALUES ('299', '141000000000', '141025000000', '古县');
INSERT INTO `j_position_county` VALUES ('300', '141000000000', '141026000000', '安泽县');
INSERT INTO `j_position_county` VALUES ('301', '141000000000', '141027000000', '浮山县');
INSERT INTO `j_position_county` VALUES ('302', '141000000000', '141028000000', '吉县');
INSERT INTO `j_position_county` VALUES ('303', '141000000000', '141029000000', '乡宁县');
INSERT INTO `j_position_county` VALUES ('304', '141000000000', '141030000000', '大宁县');
INSERT INTO `j_position_county` VALUES ('305', '141000000000', '141031000000', '隰县');
INSERT INTO `j_position_county` VALUES ('306', '141000000000', '141032000000', '永和县');
INSERT INTO `j_position_county` VALUES ('307', '141000000000', '141033000000', '蒲县');
INSERT INTO `j_position_county` VALUES ('308', '141000000000', '141034000000', '汾西县');
INSERT INTO `j_position_county` VALUES ('309', '141000000000', '141081000000', '侯马市');
INSERT INTO `j_position_county` VALUES ('310', '141000000000', '141082000000', '霍州市');
INSERT INTO `j_position_county` VALUES ('311', '141100000000', '141102000000', '离石区');
INSERT INTO `j_position_county` VALUES ('312', '141100000000', '141121000000', '文水县');
INSERT INTO `j_position_county` VALUES ('313', '141100000000', '141122000000', '交城县');
INSERT INTO `j_position_county` VALUES ('314', '141100000000', '141123000000', '兴县');
INSERT INTO `j_position_county` VALUES ('315', '141100000000', '141124000000', '临县');
INSERT INTO `j_position_county` VALUES ('316', '141100000000', '141125000000', '柳林县');
INSERT INTO `j_position_county` VALUES ('317', '141100000000', '141126000000', '石楼县');
INSERT INTO `j_position_county` VALUES ('318', '141100000000', '141127000000', '岚县');
INSERT INTO `j_position_county` VALUES ('319', '141100000000', '141128000000', '方山县');
INSERT INTO `j_position_county` VALUES ('320', '141100000000', '141129000000', '中阳县');
INSERT INTO `j_position_county` VALUES ('321', '141100000000', '141130000000', '交口县');
INSERT INTO `j_position_county` VALUES ('322', '141100000000', '141181000000', '孝义市');
INSERT INTO `j_position_county` VALUES ('323', '141100000000', '141182000000', '汾阳市');
INSERT INTO `j_position_county` VALUES ('324', '150100000000', '150102000000', '新城区');
INSERT INTO `j_position_county` VALUES ('325', '150100000000', '150103000000', '回民区');
INSERT INTO `j_position_county` VALUES ('326', '150100000000', '150104000000', '玉泉区');
INSERT INTO `j_position_county` VALUES ('327', '150100000000', '150105000000', '赛罕区');
INSERT INTO `j_position_county` VALUES ('328', '150100000000', '150121000000', '土默特左旗');
INSERT INTO `j_position_county` VALUES ('329', '150100000000', '150122000000', '托克托县');
INSERT INTO `j_position_county` VALUES ('330', '150100000000', '150123000000', '和林格尔县');
INSERT INTO `j_position_county` VALUES ('331', '150100000000', '150124000000', '清水河县');
INSERT INTO `j_position_county` VALUES ('332', '150100000000', '150125000000', '武川县');
INSERT INTO `j_position_county` VALUES ('333', '150200000000', '150202000000', '东河区');
INSERT INTO `j_position_county` VALUES ('334', '150200000000', '150203000000', '昆都仑区');
INSERT INTO `j_position_county` VALUES ('335', '150200000000', '150204000000', '青山区');
INSERT INTO `j_position_county` VALUES ('336', '150200000000', '150205000000', '石拐区');
INSERT INTO `j_position_county` VALUES ('337', '150200000000', '150206000000', '白云鄂博矿区');
INSERT INTO `j_position_county` VALUES ('338', '150200000000', '150207000000', '九原区');
INSERT INTO `j_position_county` VALUES ('339', '150200000000', '150221000000', '土默特右旗');
INSERT INTO `j_position_county` VALUES ('340', '150200000000', '150222000000', '固阳县');
INSERT INTO `j_position_county` VALUES ('341', '150200000000', '150223000000', '达尔罕茂明安联合旗');
INSERT INTO `j_position_county` VALUES ('342', '150300000000', '150302000000', '海勃湾区');
INSERT INTO `j_position_county` VALUES ('343', '150300000000', '150303000000', '海南区');
INSERT INTO `j_position_county` VALUES ('344', '150300000000', '150304000000', '乌达区');
INSERT INTO `j_position_county` VALUES ('345', '150400000000', '150402000000', '红山区');
INSERT INTO `j_position_county` VALUES ('346', '150400000000', '150403000000', '元宝山区');
INSERT INTO `j_position_county` VALUES ('347', '150400000000', '150404000000', '松山区');
INSERT INTO `j_position_county` VALUES ('348', '150400000000', '150421000000', '阿鲁科尔沁旗');
INSERT INTO `j_position_county` VALUES ('349', '150400000000', '150422000000', '巴林左旗');
INSERT INTO `j_position_county` VALUES ('350', '150400000000', '150423000000', '巴林右旗');
INSERT INTO `j_position_county` VALUES ('351', '150400000000', '150424000000', '林西县');
INSERT INTO `j_position_county` VALUES ('352', '150400000000', '150425000000', '克什克腾旗');
INSERT INTO `j_position_county` VALUES ('353', '150400000000', '150426000000', '翁牛特旗');
INSERT INTO `j_position_county` VALUES ('354', '150400000000', '150428000000', '喀喇沁旗');
INSERT INTO `j_position_county` VALUES ('355', '150400000000', '150429000000', '宁城县');
INSERT INTO `j_position_county` VALUES ('356', '150400000000', '150430000000', '敖汉旗');
INSERT INTO `j_position_county` VALUES ('357', '150500000000', '150502000000', '科尔沁区');
INSERT INTO `j_position_county` VALUES ('358', '150500000000', '150521000000', '科尔沁左翼中旗');
INSERT INTO `j_position_county` VALUES ('359', '150500000000', '150522000000', '科尔沁左翼后旗');
INSERT INTO `j_position_county` VALUES ('360', '150500000000', '150523000000', '开鲁县');
INSERT INTO `j_position_county` VALUES ('361', '150500000000', '150524000000', '库伦旗');
INSERT INTO `j_position_county` VALUES ('362', '150500000000', '150525000000', '奈曼旗');
INSERT INTO `j_position_county` VALUES ('363', '150500000000', '150526000000', '扎鲁特旗');
INSERT INTO `j_position_county` VALUES ('364', '150500000000', '150581000000', '霍林郭勒市');
INSERT INTO `j_position_county` VALUES ('365', '150600000000', '150602000000', '东胜区');
INSERT INTO `j_position_county` VALUES ('366', '150600000000', '150621000000', '达拉特旗');
INSERT INTO `j_position_county` VALUES ('367', '150600000000', '150622000000', '准格尔旗');
INSERT INTO `j_position_county` VALUES ('368', '150600000000', '150623000000', '鄂托克前旗');
INSERT INTO `j_position_county` VALUES ('369', '150600000000', '150624000000', '鄂托克旗');
INSERT INTO `j_position_county` VALUES ('370', '150600000000', '150625000000', '杭锦旗');
INSERT INTO `j_position_county` VALUES ('371', '150600000000', '150626000000', '乌审旗');
INSERT INTO `j_position_county` VALUES ('372', '150600000000', '150627000000', '伊金霍洛旗');
INSERT INTO `j_position_county` VALUES ('373', '150700000000', '150702000000', '海拉尔区');
INSERT INTO `j_position_county` VALUES ('374', '150700000000', '150703000000', '扎赉诺尔区');
INSERT INTO `j_position_county` VALUES ('375', '150700000000', '150721000000', '阿荣旗');
INSERT INTO `j_position_county` VALUES ('376', '150700000000', '150722000000', '莫力达瓦达斡尔族自治旗');
INSERT INTO `j_position_county` VALUES ('377', '150700000000', '150723000000', '鄂伦春自治旗');
INSERT INTO `j_position_county` VALUES ('378', '150700000000', '150724000000', '鄂温克族自治旗');
INSERT INTO `j_position_county` VALUES ('379', '150700000000', '150725000000', '陈巴尔虎旗');
INSERT INTO `j_position_county` VALUES ('380', '150700000000', '150726000000', '新巴尔虎左旗');
INSERT INTO `j_position_county` VALUES ('381', '150700000000', '150727000000', '新巴尔虎右旗');
INSERT INTO `j_position_county` VALUES ('382', '150700000000', '150781000000', '满洲里市');
INSERT INTO `j_position_county` VALUES ('383', '150700000000', '150782000000', '牙克石市');
INSERT INTO `j_position_county` VALUES ('384', '150700000000', '150783000000', '扎兰屯市');
INSERT INTO `j_position_county` VALUES ('385', '150700000000', '150784000000', '额尔古纳市');
INSERT INTO `j_position_county` VALUES ('386', '150700000000', '150785000000', '根河市');
INSERT INTO `j_position_county` VALUES ('387', '150800000000', '150802000000', '临河区');
INSERT INTO `j_position_county` VALUES ('388', '150800000000', '150821000000', '五原县');
INSERT INTO `j_position_county` VALUES ('389', '150800000000', '150822000000', '磴口县');
INSERT INTO `j_position_county` VALUES ('390', '150800000000', '150823000000', '乌拉特前旗');
INSERT INTO `j_position_county` VALUES ('391', '150800000000', '150824000000', '乌拉特中旗');
INSERT INTO `j_position_county` VALUES ('392', '150800000000', '150825000000', '乌拉特后旗');
INSERT INTO `j_position_county` VALUES ('393', '150800000000', '150826000000', '杭锦后旗');
INSERT INTO `j_position_county` VALUES ('394', '150900000000', '150902000000', '集宁区');
INSERT INTO `j_position_county` VALUES ('395', '150900000000', '150921000000', '卓资县');
INSERT INTO `j_position_county` VALUES ('396', '150900000000', '150922000000', '化德县');
INSERT INTO `j_position_county` VALUES ('397', '150900000000', '150923000000', '商都县');
INSERT INTO `j_position_county` VALUES ('398', '150900000000', '150924000000', '兴和县');
INSERT INTO `j_position_county` VALUES ('399', '150900000000', '150925000000', '凉城县');
INSERT INTO `j_position_county` VALUES ('400', '150900000000', '150926000000', '察哈尔右翼前旗');
INSERT INTO `j_position_county` VALUES ('401', '150900000000', '150927000000', '察哈尔右翼中旗');
INSERT INTO `j_position_county` VALUES ('402', '150900000000', '150928000000', '察哈尔右翼后旗');
INSERT INTO `j_position_county` VALUES ('403', '150900000000', '150929000000', '四子王旗');
INSERT INTO `j_position_county` VALUES ('404', '150900000000', '150981000000', '丰镇市');
INSERT INTO `j_position_county` VALUES ('405', '152200000000', '152201000000', '乌兰浩特市');
INSERT INTO `j_position_county` VALUES ('406', '152200000000', '152202000000', '阿尔山市');
INSERT INTO `j_position_county` VALUES ('407', '152200000000', '152221000000', '科尔沁右翼前旗');
INSERT INTO `j_position_county` VALUES ('408', '152200000000', '152222000000', '科尔沁右翼中旗');
INSERT INTO `j_position_county` VALUES ('409', '152200000000', '152223000000', '扎赉特旗');
INSERT INTO `j_position_county` VALUES ('410', '152200000000', '152224000000', '突泉县');
INSERT INTO `j_position_county` VALUES ('411', '152500000000', '152501000000', '二连浩特市');
INSERT INTO `j_position_county` VALUES ('412', '152500000000', '152502000000', '锡林浩特市');
INSERT INTO `j_position_county` VALUES ('413', '152500000000', '152522000000', '阿巴嘎旗');
INSERT INTO `j_position_county` VALUES ('414', '152500000000', '152523000000', '苏尼特左旗');
INSERT INTO `j_position_county` VALUES ('415', '152500000000', '152524000000', '苏尼特右旗');
INSERT INTO `j_position_county` VALUES ('416', '152500000000', '152525000000', '东乌珠穆沁旗');
INSERT INTO `j_position_county` VALUES ('417', '152500000000', '152526000000', '西乌珠穆沁旗');
INSERT INTO `j_position_county` VALUES ('418', '152500000000', '152527000000', '太仆寺旗');
INSERT INTO `j_position_county` VALUES ('419', '152500000000', '152528000000', '镶黄旗');
INSERT INTO `j_position_county` VALUES ('420', '152500000000', '152529000000', '正镶白旗');
INSERT INTO `j_position_county` VALUES ('421', '152500000000', '152530000000', '正蓝旗');
INSERT INTO `j_position_county` VALUES ('422', '152500000000', '152531000000', '多伦县');
INSERT INTO `j_position_county` VALUES ('423', '152900000000', '152921000000', '阿拉善左旗');
INSERT INTO `j_position_county` VALUES ('424', '152900000000', '152922000000', '阿拉善右旗');
INSERT INTO `j_position_county` VALUES ('425', '152900000000', '152923000000', '额济纳旗');
INSERT INTO `j_position_county` VALUES ('426', '210100000000', '210102000000', '和平区');
INSERT INTO `j_position_county` VALUES ('427', '210100000000', '210103000000', '沈河区');
INSERT INTO `j_position_county` VALUES ('428', '210100000000', '210104000000', '大东区');
INSERT INTO `j_position_county` VALUES ('429', '210100000000', '210105000000', '皇姑区');
INSERT INTO `j_position_county` VALUES ('430', '210100000000', '210106000000', '铁西区');
INSERT INTO `j_position_county` VALUES ('431', '210100000000', '210111000000', '苏家屯区');
INSERT INTO `j_position_county` VALUES ('432', '210100000000', '210112000000', '东陵区');
INSERT INTO `j_position_county` VALUES ('433', '210100000000', '210113000000', '沈北新区');
INSERT INTO `j_position_county` VALUES ('434', '210100000000', '210114000000', '于洪区');
INSERT INTO `j_position_county` VALUES ('435', '210100000000', '210122000000', '辽中县');
INSERT INTO `j_position_county` VALUES ('436', '210100000000', '210123000000', '康平县');
INSERT INTO `j_position_county` VALUES ('437', '210100000000', '210124000000', '法库县');
INSERT INTO `j_position_county` VALUES ('438', '210100000000', '210181000000', '新民市');
INSERT INTO `j_position_county` VALUES ('439', '210200000000', '210202000000', '中山区');
INSERT INTO `j_position_county` VALUES ('440', '210200000000', '210203000000', '西岗区');
INSERT INTO `j_position_county` VALUES ('441', '210200000000', '210204000000', '沙河口区');
INSERT INTO `j_position_county` VALUES ('442', '210200000000', '210211000000', '甘井子区');
INSERT INTO `j_position_county` VALUES ('443', '210200000000', '210212000000', '旅顺口区');
INSERT INTO `j_position_county` VALUES ('444', '210200000000', '210213000000', '金州区');
INSERT INTO `j_position_county` VALUES ('445', '210200000000', '210224000000', '长海县');
INSERT INTO `j_position_county` VALUES ('446', '210200000000', '210281000000', '瓦房店市');
INSERT INTO `j_position_county` VALUES ('447', '210200000000', '210282000000', '普兰店市');
INSERT INTO `j_position_county` VALUES ('448', '210200000000', '210283000000', '庄河市');
INSERT INTO `j_position_county` VALUES ('449', '210300000000', '210302000000', '铁东区');
INSERT INTO `j_position_county` VALUES ('450', '210300000000', '210303000000', '铁西区');
INSERT INTO `j_position_county` VALUES ('451', '210300000000', '210304000000', '立山区');
INSERT INTO `j_position_county` VALUES ('452', '210300000000', '210311000000', '千山区');
INSERT INTO `j_position_county` VALUES ('453', '210300000000', '210321000000', '台安县');
INSERT INTO `j_position_county` VALUES ('454', '210300000000', '210323000000', '岫岩满族自治县');
INSERT INTO `j_position_county` VALUES ('455', '210300000000', '210381000000', '海城市');
INSERT INTO `j_position_county` VALUES ('456', '210400000000', '210402000000', '新抚区');
INSERT INTO `j_position_county` VALUES ('457', '210400000000', '210403000000', '东洲区');
INSERT INTO `j_position_county` VALUES ('458', '210400000000', '210404000000', '望花区');
INSERT INTO `j_position_county` VALUES ('459', '210400000000', '210411000000', '顺城区');
INSERT INTO `j_position_county` VALUES ('460', '210400000000', '210421000000', '抚顺县');
INSERT INTO `j_position_county` VALUES ('461', '210400000000', '210422000000', '新宾满族自治县');
INSERT INTO `j_position_county` VALUES ('462', '210400000000', '210423000000', '清原满族自治县');
INSERT INTO `j_position_county` VALUES ('463', '210500000000', '210502000000', '平山区');
INSERT INTO `j_position_county` VALUES ('464', '210500000000', '210503000000', '溪湖区');
INSERT INTO `j_position_county` VALUES ('465', '210500000000', '210504000000', '明山区');
INSERT INTO `j_position_county` VALUES ('466', '210500000000', '210505000000', '南芬区');
INSERT INTO `j_position_county` VALUES ('467', '210500000000', '210521000000', '本溪满族自治县');
INSERT INTO `j_position_county` VALUES ('468', '210500000000', '210522000000', '桓仁满族自治县');
INSERT INTO `j_position_county` VALUES ('469', '210600000000', '210602000000', '元宝区');
INSERT INTO `j_position_county` VALUES ('470', '210600000000', '210603000000', '振兴区');
INSERT INTO `j_position_county` VALUES ('471', '210600000000', '210604000000', '振安区');
INSERT INTO `j_position_county` VALUES ('472', '210600000000', '210624000000', '宽甸满族自治县');
INSERT INTO `j_position_county` VALUES ('473', '210600000000', '210681000000', '东港市');
INSERT INTO `j_position_county` VALUES ('474', '210600000000', '210682000000', '凤城市');
INSERT INTO `j_position_county` VALUES ('475', '210700000000', '210702000000', '古塔区');
INSERT INTO `j_position_county` VALUES ('476', '210700000000', '210703000000', '凌河区');
INSERT INTO `j_position_county` VALUES ('477', '210700000000', '210711000000', '太和区');
INSERT INTO `j_position_county` VALUES ('478', '210700000000', '210726000000', '黑山县');
INSERT INTO `j_position_county` VALUES ('479', '210700000000', '210727000000', '义县');
INSERT INTO `j_position_county` VALUES ('480', '210700000000', '210781000000', '凌海市');
INSERT INTO `j_position_county` VALUES ('481', '210700000000', '210782000000', '北镇市');
INSERT INTO `j_position_county` VALUES ('482', '210800000000', '210802000000', '站前区');
INSERT INTO `j_position_county` VALUES ('483', '210800000000', '210803000000', '西市区');
INSERT INTO `j_position_county` VALUES ('484', '210800000000', '210804000000', '鲅鱼圈区');
INSERT INTO `j_position_county` VALUES ('485', '210800000000', '210811000000', '老边区');
INSERT INTO `j_position_county` VALUES ('486', '210800000000', '210881000000', '盖州市');
INSERT INTO `j_position_county` VALUES ('487', '210800000000', '210882000000', '大石桥市');
INSERT INTO `j_position_county` VALUES ('488', '210900000000', '210902000000', '海州区');
INSERT INTO `j_position_county` VALUES ('489', '210900000000', '210903000000', '新邱区');
INSERT INTO `j_position_county` VALUES ('490', '210900000000', '210904000000', '太平区');
INSERT INTO `j_position_county` VALUES ('491', '210900000000', '210905000000', '清河门区');
INSERT INTO `j_position_county` VALUES ('492', '210900000000', '210911000000', '细河区');
INSERT INTO `j_position_county` VALUES ('493', '210900000000', '210921000000', '阜新蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('494', '210900000000', '210922000000', '彰武县');
INSERT INTO `j_position_county` VALUES ('495', '211000000000', '211002000000', '白塔区');
INSERT INTO `j_position_county` VALUES ('496', '211000000000', '211003000000', '文圣区');
INSERT INTO `j_position_county` VALUES ('497', '211000000000', '211004000000', '宏伟区');
INSERT INTO `j_position_county` VALUES ('498', '211000000000', '211005000000', '弓长岭区');
INSERT INTO `j_position_county` VALUES ('499', '211000000000', '211011000000', '太子河区');
INSERT INTO `j_position_county` VALUES ('500', '211000000000', '211021000000', '辽阳县');
INSERT INTO `j_position_county` VALUES ('501', '211000000000', '211081000000', '灯塔市');
INSERT INTO `j_position_county` VALUES ('502', '211100000000', '211102000000', '双台子区');
INSERT INTO `j_position_county` VALUES ('503', '211100000000', '211103000000', '兴隆台区');
INSERT INTO `j_position_county` VALUES ('504', '211100000000', '211121000000', '大洼县');
INSERT INTO `j_position_county` VALUES ('505', '211100000000', '211122000000', '盘山县');
INSERT INTO `j_position_county` VALUES ('506', '211200000000', '211202000000', '银州区');
INSERT INTO `j_position_county` VALUES ('507', '211200000000', '211204000000', '清河区');
INSERT INTO `j_position_county` VALUES ('508', '211200000000', '211221000000', '铁岭县');
INSERT INTO `j_position_county` VALUES ('509', '211200000000', '211223000000', '西丰县');
INSERT INTO `j_position_county` VALUES ('510', '211200000000', '211224000000', '昌图县');
INSERT INTO `j_position_county` VALUES ('511', '211200000000', '211281000000', '调兵山市');
INSERT INTO `j_position_county` VALUES ('512', '211200000000', '211282000000', '开原市');
INSERT INTO `j_position_county` VALUES ('513', '211300000000', '211302000000', '双塔区');
INSERT INTO `j_position_county` VALUES ('514', '211300000000', '211303000000', '龙城区');
INSERT INTO `j_position_county` VALUES ('515', '211300000000', '211321000000', '朝阳县');
INSERT INTO `j_position_county` VALUES ('516', '211300000000', '211322000000', '建平县');
INSERT INTO `j_position_county` VALUES ('517', '211300000000', '211324000000', '喀喇沁左翼蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('518', '211300000000', '211381000000', '北票市');
INSERT INTO `j_position_county` VALUES ('519', '211300000000', '211382000000', '凌源市');
INSERT INTO `j_position_county` VALUES ('520', '211400000000', '211402000000', '连山区');
INSERT INTO `j_position_county` VALUES ('521', '211400000000', '211403000000', '龙港区');
INSERT INTO `j_position_county` VALUES ('522', '211400000000', '211404000000', '南票区');
INSERT INTO `j_position_county` VALUES ('523', '211400000000', '211421000000', '绥中县');
INSERT INTO `j_position_county` VALUES ('524', '211400000000', '211422000000', '建昌县');
INSERT INTO `j_position_county` VALUES ('525', '211400000000', '211481000000', '兴城市');
INSERT INTO `j_position_county` VALUES ('526', '220100000000', '220102000000', '南关区');
INSERT INTO `j_position_county` VALUES ('527', '220100000000', '220103000000', '宽城区');
INSERT INTO `j_position_county` VALUES ('528', '220100000000', '220104000000', '朝阳区');
INSERT INTO `j_position_county` VALUES ('529', '220100000000', '220105000000', '二道区');
INSERT INTO `j_position_county` VALUES ('530', '220100000000', '220106000000', '绿园区');
INSERT INTO `j_position_county` VALUES ('531', '220100000000', '220112000000', '双阳区');
INSERT INTO `j_position_county` VALUES ('532', '220100000000', '220122000000', '农安县');
INSERT INTO `j_position_county` VALUES ('533', '220100000000', '220181000000', '九台市');
INSERT INTO `j_position_county` VALUES ('534', '220100000000', '220182000000', '榆树市');
INSERT INTO `j_position_county` VALUES ('535', '220100000000', '220183000000', '德惠市');
INSERT INTO `j_position_county` VALUES ('536', '220200000000', '220202000000', '昌邑区');
INSERT INTO `j_position_county` VALUES ('537', '220200000000', '220203000000', '龙潭区');
INSERT INTO `j_position_county` VALUES ('538', '220200000000', '220204000000', '船营区');
INSERT INTO `j_position_county` VALUES ('539', '220200000000', '220211000000', '丰满区');
INSERT INTO `j_position_county` VALUES ('540', '220200000000', '220221000000', '永吉县');
INSERT INTO `j_position_county` VALUES ('541', '220200000000', '220281000000', '蛟河市');
INSERT INTO `j_position_county` VALUES ('542', '220200000000', '220282000000', '桦甸市');
INSERT INTO `j_position_county` VALUES ('543', '220200000000', '220283000000', '舒兰市');
INSERT INTO `j_position_county` VALUES ('544', '220200000000', '220284000000', '磐石市');
INSERT INTO `j_position_county` VALUES ('545', '220300000000', '220302000000', '铁西区');
INSERT INTO `j_position_county` VALUES ('546', '220300000000', '220303000000', '铁东区');
INSERT INTO `j_position_county` VALUES ('547', '220300000000', '220322000000', '梨树县');
INSERT INTO `j_position_county` VALUES ('548', '220300000000', '220323000000', '伊通满族自治县');
INSERT INTO `j_position_county` VALUES ('549', '220300000000', '220381000000', '公主岭市');
INSERT INTO `j_position_county` VALUES ('550', '220300000000', '220382000000', '双辽市');
INSERT INTO `j_position_county` VALUES ('551', '220400000000', '220402000000', '龙山区');
INSERT INTO `j_position_county` VALUES ('552', '220400000000', '220403000000', '西安区');
INSERT INTO `j_position_county` VALUES ('553', '220400000000', '220421000000', '东丰县');
INSERT INTO `j_position_county` VALUES ('554', '220400000000', '220422000000', '东辽县');
INSERT INTO `j_position_county` VALUES ('555', '220500000000', '220502000000', '东昌区');
INSERT INTO `j_position_county` VALUES ('556', '220500000000', '220503000000', '二道江区');
INSERT INTO `j_position_county` VALUES ('557', '220500000000', '220521000000', '通化县');
INSERT INTO `j_position_county` VALUES ('558', '220500000000', '220523000000', '辉南县');
INSERT INTO `j_position_county` VALUES ('559', '220500000000', '220524000000', '柳河县');
INSERT INTO `j_position_county` VALUES ('560', '220500000000', '220581000000', '梅河口市');
INSERT INTO `j_position_county` VALUES ('561', '220500000000', '220582000000', '集安市');
INSERT INTO `j_position_county` VALUES ('562', '220600000000', '220602000000', '浑江区');
INSERT INTO `j_position_county` VALUES ('563', '220600000000', '220605000000', '江源区');
INSERT INTO `j_position_county` VALUES ('564', '220600000000', '220621000000', '抚松县');
INSERT INTO `j_position_county` VALUES ('565', '220600000000', '220622000000', '靖宇县');
INSERT INTO `j_position_county` VALUES ('566', '220600000000', '220623000000', '长白朝鲜族自治县');
INSERT INTO `j_position_county` VALUES ('567', '220600000000', '220681000000', '临江市');
INSERT INTO `j_position_county` VALUES ('568', '220700000000', '220702000000', '宁江区');
INSERT INTO `j_position_county` VALUES ('569', '220700000000', '220721000000', '前郭尔罗斯蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('570', '220700000000', '220722000000', '长岭县');
INSERT INTO `j_position_county` VALUES ('571', '220700000000', '220723000000', '乾安县');
INSERT INTO `j_position_county` VALUES ('572', '220700000000', '220781000000', '扶余市');
INSERT INTO `j_position_county` VALUES ('573', '220800000000', '220802000000', '洮北区');
INSERT INTO `j_position_county` VALUES ('574', '220800000000', '220821000000', '镇赉县');
INSERT INTO `j_position_county` VALUES ('575', '220800000000', '220822000000', '通榆县');
INSERT INTO `j_position_county` VALUES ('576', '220800000000', '220881000000', '洮南市');
INSERT INTO `j_position_county` VALUES ('577', '220800000000', '220882000000', '大安市');
INSERT INTO `j_position_county` VALUES ('578', '222400000000', '222401000000', '延吉市');
INSERT INTO `j_position_county` VALUES ('579', '222400000000', '222402000000', '图们市');
INSERT INTO `j_position_county` VALUES ('580', '222400000000', '222403000000', '敦化市');
INSERT INTO `j_position_county` VALUES ('581', '222400000000', '222404000000', '珲春市');
INSERT INTO `j_position_county` VALUES ('582', '222400000000', '222405000000', '龙井市');
INSERT INTO `j_position_county` VALUES ('583', '222400000000', '222406000000', '和龙市');
INSERT INTO `j_position_county` VALUES ('584', '222400000000', '222424000000', '汪清县');
INSERT INTO `j_position_county` VALUES ('585', '222400000000', '222426000000', '安图县');
INSERT INTO `j_position_county` VALUES ('586', '230100000000', '230102000000', '道里区');
INSERT INTO `j_position_county` VALUES ('587', '230100000000', '230103000000', '南岗区');
INSERT INTO `j_position_county` VALUES ('588', '230100000000', '230104000000', '道外区');
INSERT INTO `j_position_county` VALUES ('589', '230100000000', '230108000000', '平房区');
INSERT INTO `j_position_county` VALUES ('590', '230100000000', '230109000000', '松北区');
INSERT INTO `j_position_county` VALUES ('591', '230100000000', '230110000000', '香坊区');
INSERT INTO `j_position_county` VALUES ('592', '230100000000', '230111000000', '呼兰区');
INSERT INTO `j_position_county` VALUES ('593', '230100000000', '230112000000', '阿城区');
INSERT INTO `j_position_county` VALUES ('594', '230100000000', '230123000000', '依兰县');
INSERT INTO `j_position_county` VALUES ('595', '230100000000', '230124000000', '方正县');
INSERT INTO `j_position_county` VALUES ('596', '230100000000', '230125000000', '宾县');
INSERT INTO `j_position_county` VALUES ('597', '230100000000', '230126000000', '巴彦县');
INSERT INTO `j_position_county` VALUES ('598', '230100000000', '230127000000', '木兰县');
INSERT INTO `j_position_county` VALUES ('599', '230100000000', '230128000000', '通河县');
INSERT INTO `j_position_county` VALUES ('600', '230100000000', '230129000000', '延寿县');
INSERT INTO `j_position_county` VALUES ('601', '230100000000', '230182000000', '双城市');
INSERT INTO `j_position_county` VALUES ('602', '230100000000', '230183000000', '尚志市');
INSERT INTO `j_position_county` VALUES ('603', '230100000000', '230184000000', '五常市');
INSERT INTO `j_position_county` VALUES ('604', '230200000000', '230202000000', '龙沙区');
INSERT INTO `j_position_county` VALUES ('605', '230200000000', '230203000000', '建华区');
INSERT INTO `j_position_county` VALUES ('606', '230200000000', '230204000000', '铁锋区');
INSERT INTO `j_position_county` VALUES ('607', '230200000000', '230205000000', '昂昂溪区');
INSERT INTO `j_position_county` VALUES ('608', '230200000000', '230206000000', '富拉尔基区');
INSERT INTO `j_position_county` VALUES ('609', '230200000000', '230207000000', '碾子山区');
INSERT INTO `j_position_county` VALUES ('610', '230200000000', '230208000000', '梅里斯达斡尔族区');
INSERT INTO `j_position_county` VALUES ('611', '230200000000', '230221000000', '龙江县');
INSERT INTO `j_position_county` VALUES ('612', '230200000000', '230223000000', '依安县');
INSERT INTO `j_position_county` VALUES ('613', '230200000000', '230224000000', '泰来县');
INSERT INTO `j_position_county` VALUES ('614', '230200000000', '230225000000', '甘南县');
INSERT INTO `j_position_county` VALUES ('615', '230200000000', '230227000000', '富裕县');
INSERT INTO `j_position_county` VALUES ('616', '230200000000', '230229000000', '克山县');
INSERT INTO `j_position_county` VALUES ('617', '230200000000', '230230000000', '克东县');
INSERT INTO `j_position_county` VALUES ('618', '230200000000', '230231000000', '拜泉县');
INSERT INTO `j_position_county` VALUES ('619', '230200000000', '230281000000', '讷河市');
INSERT INTO `j_position_county` VALUES ('620', '230300000000', '230302000000', '鸡冠区');
INSERT INTO `j_position_county` VALUES ('621', '230300000000', '230303000000', '恒山区');
INSERT INTO `j_position_county` VALUES ('622', '230300000000', '230304000000', '滴道区');
INSERT INTO `j_position_county` VALUES ('623', '230300000000', '230305000000', '梨树区');
INSERT INTO `j_position_county` VALUES ('624', '230300000000', '230306000000', '城子河区');
INSERT INTO `j_position_county` VALUES ('625', '230300000000', '230307000000', '麻山区');
INSERT INTO `j_position_county` VALUES ('626', '230300000000', '230321000000', '鸡东县');
INSERT INTO `j_position_county` VALUES ('627', '230300000000', '230381000000', '虎林市');
INSERT INTO `j_position_county` VALUES ('628', '230300000000', '230382000000', '密山市');
INSERT INTO `j_position_county` VALUES ('629', '230400000000', '230402000000', '向阳区');
INSERT INTO `j_position_county` VALUES ('630', '230400000000', '230403000000', '工农区');
INSERT INTO `j_position_county` VALUES ('631', '230400000000', '230404000000', '南山区');
INSERT INTO `j_position_county` VALUES ('632', '230400000000', '230405000000', '兴安区');
INSERT INTO `j_position_county` VALUES ('633', '230400000000', '230406000000', '东山区');
INSERT INTO `j_position_county` VALUES ('634', '230400000000', '230407000000', '兴山区');
INSERT INTO `j_position_county` VALUES ('635', '230400000000', '230421000000', '萝北县');
INSERT INTO `j_position_county` VALUES ('636', '230400000000', '230422000000', '绥滨县');
INSERT INTO `j_position_county` VALUES ('637', '230500000000', '230502000000', '尖山区');
INSERT INTO `j_position_county` VALUES ('638', '230500000000', '230503000000', '岭东区');
INSERT INTO `j_position_county` VALUES ('639', '230500000000', '230505000000', '四方台区');
INSERT INTO `j_position_county` VALUES ('640', '230500000000', '230506000000', '宝山区');
INSERT INTO `j_position_county` VALUES ('641', '230500000000', '230521000000', '集贤县');
INSERT INTO `j_position_county` VALUES ('642', '230500000000', '230522000000', '友谊县');
INSERT INTO `j_position_county` VALUES ('643', '230500000000', '230523000000', '宝清县');
INSERT INTO `j_position_county` VALUES ('644', '230500000000', '230524000000', '饶河县');
INSERT INTO `j_position_county` VALUES ('645', '230600000000', '230602000000', '萨尔图区');
INSERT INTO `j_position_county` VALUES ('646', '230600000000', '230603000000', '龙凤区');
INSERT INTO `j_position_county` VALUES ('647', '230600000000', '230604000000', '让胡路区');
INSERT INTO `j_position_county` VALUES ('648', '230600000000', '230605000000', '红岗区');
INSERT INTO `j_position_county` VALUES ('649', '230600000000', '230606000000', '大同区');
INSERT INTO `j_position_county` VALUES ('650', '230600000000', '230621000000', '肇州县');
INSERT INTO `j_position_county` VALUES ('651', '230600000000', '230622000000', '肇源县');
INSERT INTO `j_position_county` VALUES ('652', '230600000000', '230623000000', '林甸县');
INSERT INTO `j_position_county` VALUES ('653', '230600000000', '230624000000', '杜尔伯特蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('654', '230700000000', '230702000000', '伊春区');
INSERT INTO `j_position_county` VALUES ('655', '230700000000', '230703000000', '南岔区');
INSERT INTO `j_position_county` VALUES ('656', '230700000000', '230704000000', '友好区');
INSERT INTO `j_position_county` VALUES ('657', '230700000000', '230705000000', '西林区');
INSERT INTO `j_position_county` VALUES ('658', '230700000000', '230706000000', '翠峦区');
INSERT INTO `j_position_county` VALUES ('659', '230700000000', '230707000000', '新青区');
INSERT INTO `j_position_county` VALUES ('660', '230700000000', '230708000000', '美溪区');
INSERT INTO `j_position_county` VALUES ('661', '230700000000', '230709000000', '金山屯区');
INSERT INTO `j_position_county` VALUES ('662', '230700000000', '230710000000', '五营区');
INSERT INTO `j_position_county` VALUES ('663', '230700000000', '230711000000', '乌马河区');
INSERT INTO `j_position_county` VALUES ('664', '230700000000', '230712000000', '汤旺河区');
INSERT INTO `j_position_county` VALUES ('665', '230700000000', '230713000000', '带岭区');
INSERT INTO `j_position_county` VALUES ('666', '230700000000', '230714000000', '乌伊岭区');
INSERT INTO `j_position_county` VALUES ('667', '230700000000', '230715000000', '红星区');
INSERT INTO `j_position_county` VALUES ('668', '230700000000', '230716000000', '上甘岭区');
INSERT INTO `j_position_county` VALUES ('669', '230700000000', '230722000000', '嘉荫县');
INSERT INTO `j_position_county` VALUES ('670', '230700000000', '230781000000', '铁力市');
INSERT INTO `j_position_county` VALUES ('671', '230800000000', '230803000000', '向阳区');
INSERT INTO `j_position_county` VALUES ('672', '230800000000', '230804000000', '前进区');
INSERT INTO `j_position_county` VALUES ('673', '230800000000', '230805000000', '东风区');
INSERT INTO `j_position_county` VALUES ('674', '230800000000', '230811000000', '郊区');
INSERT INTO `j_position_county` VALUES ('675', '230800000000', '230822000000', '桦南县');
INSERT INTO `j_position_county` VALUES ('676', '230800000000', '230826000000', '桦川县');
INSERT INTO `j_position_county` VALUES ('677', '230800000000', '230828000000', '汤原县');
INSERT INTO `j_position_county` VALUES ('678', '230800000000', '230833000000', '抚远县');
INSERT INTO `j_position_county` VALUES ('679', '230800000000', '230881000000', '同江市');
INSERT INTO `j_position_county` VALUES ('680', '230800000000', '230882000000', '富锦市');
INSERT INTO `j_position_county` VALUES ('681', '230900000000', '230902000000', '新兴区');
INSERT INTO `j_position_county` VALUES ('682', '230900000000', '230903000000', '桃山区');
INSERT INTO `j_position_county` VALUES ('683', '230900000000', '230904000000', '茄子河区');
INSERT INTO `j_position_county` VALUES ('684', '230900000000', '230921000000', '勃利县');
INSERT INTO `j_position_county` VALUES ('685', '231000000000', '231002000000', '东安区');
INSERT INTO `j_position_county` VALUES ('686', '231000000000', '231003000000', '阳明区');
INSERT INTO `j_position_county` VALUES ('687', '231000000000', '231004000000', '爱民区');
INSERT INTO `j_position_county` VALUES ('688', '231000000000', '231005000000', '西安区');
INSERT INTO `j_position_county` VALUES ('689', '231000000000', '231024000000', '东宁县');
INSERT INTO `j_position_county` VALUES ('690', '231000000000', '231025000000', '林口县');
INSERT INTO `j_position_county` VALUES ('691', '231000000000', '231081000000', '绥芬河市');
INSERT INTO `j_position_county` VALUES ('692', '231000000000', '231083000000', '海林市');
INSERT INTO `j_position_county` VALUES ('693', '231000000000', '231084000000', '宁安市');
INSERT INTO `j_position_county` VALUES ('694', '231000000000', '231085000000', '穆棱市');
INSERT INTO `j_position_county` VALUES ('695', '231100000000', '231102000000', '爱辉区');
INSERT INTO `j_position_county` VALUES ('696', '231100000000', '231121000000', '嫩江县');
INSERT INTO `j_position_county` VALUES ('697', '231100000000', '231123000000', '逊克县');
INSERT INTO `j_position_county` VALUES ('698', '231100000000', '231124000000', '孙吴县');
INSERT INTO `j_position_county` VALUES ('699', '231100000000', '231181000000', '北安市');
INSERT INTO `j_position_county` VALUES ('700', '231100000000', '231182000000', '五大连池市');
INSERT INTO `j_position_county` VALUES ('701', '231200000000', '231202000000', '北林区');
INSERT INTO `j_position_county` VALUES ('702', '231200000000', '231221000000', '望奎县');
INSERT INTO `j_position_county` VALUES ('703', '231200000000', '231222000000', '兰西县');
INSERT INTO `j_position_county` VALUES ('704', '231200000000', '231223000000', '青冈县');
INSERT INTO `j_position_county` VALUES ('705', '231200000000', '231224000000', '庆安县');
INSERT INTO `j_position_county` VALUES ('706', '231200000000', '231225000000', '明水县');
INSERT INTO `j_position_county` VALUES ('707', '231200000000', '231226000000', '绥棱县');
INSERT INTO `j_position_county` VALUES ('708', '231200000000', '231281000000', '安达市');
INSERT INTO `j_position_county` VALUES ('709', '231200000000', '231282000000', '肇东市');
INSERT INTO `j_position_county` VALUES ('710', '231200000000', '231283000000', '海伦市');
INSERT INTO `j_position_county` VALUES ('711', '232700000000', '232721000000', '呼玛县');
INSERT INTO `j_position_county` VALUES ('712', '232700000000', '232722000000', '塔河县');
INSERT INTO `j_position_county` VALUES ('713', '232700000000', '232723000000', '漠河县');
INSERT INTO `j_position_county` VALUES ('714', '310100000000', '310101000000', '黄浦区');
INSERT INTO `j_position_county` VALUES ('715', '310100000000', '310104000000', '徐汇区');
INSERT INTO `j_position_county` VALUES ('716', '310100000000', '310105000000', '长宁区');
INSERT INTO `j_position_county` VALUES ('717', '310100000000', '310106000000', '静安区');
INSERT INTO `j_position_county` VALUES ('718', '310100000000', '310107000000', '普陀区');
INSERT INTO `j_position_county` VALUES ('719', '310100000000', '310108000000', '闸北区');
INSERT INTO `j_position_county` VALUES ('720', '310100000000', '310109000000', '虹口区');
INSERT INTO `j_position_county` VALUES ('721', '310100000000', '310110000000', '杨浦区');
INSERT INTO `j_position_county` VALUES ('722', '310100000000', '310112000000', '闵行区');
INSERT INTO `j_position_county` VALUES ('723', '310100000000', '310113000000', '宝山区');
INSERT INTO `j_position_county` VALUES ('724', '310100000000', '310114000000', '嘉定区');
INSERT INTO `j_position_county` VALUES ('725', '310100000000', '310115000000', '浦东新区');
INSERT INTO `j_position_county` VALUES ('726', '310100000000', '310116000000', '金山区');
INSERT INTO `j_position_county` VALUES ('727', '310100000000', '310117000000', '松江区');
INSERT INTO `j_position_county` VALUES ('728', '310100000000', '310118000000', '青浦区');
INSERT INTO `j_position_county` VALUES ('729', '310100000000', '310120000000', '奉贤区');
INSERT INTO `j_position_county` VALUES ('730', '310200000000', '310230000000', '崇明县');
INSERT INTO `j_position_county` VALUES ('731', '320100000000', '320102000000', '玄武区');
INSERT INTO `j_position_county` VALUES ('732', '320100000000', '320104000000', '秦淮区');
INSERT INTO `j_position_county` VALUES ('733', '320100000000', '320105000000', '建邺区');
INSERT INTO `j_position_county` VALUES ('734', '320100000000', '320106000000', '鼓楼区');
INSERT INTO `j_position_county` VALUES ('735', '320100000000', '320111000000', '浦口区');
INSERT INTO `j_position_county` VALUES ('736', '320100000000', '320113000000', '栖霞区');
INSERT INTO `j_position_county` VALUES ('737', '320100000000', '320114000000', '雨花台区');
INSERT INTO `j_position_county` VALUES ('738', '320100000000', '320115000000', '江宁区');
INSERT INTO `j_position_county` VALUES ('739', '320100000000', '320116000000', '六合区');
INSERT INTO `j_position_county` VALUES ('740', '320100000000', '320117000000', '溧水区');
INSERT INTO `j_position_county` VALUES ('741', '320100000000', '320118000000', '高淳区');
INSERT INTO `j_position_county` VALUES ('742', '320200000000', '320202000000', '崇安区');
INSERT INTO `j_position_county` VALUES ('743', '320200000000', '320203000000', '南长区');
INSERT INTO `j_position_county` VALUES ('744', '320200000000', '320204000000', '北塘区');
INSERT INTO `j_position_county` VALUES ('745', '320200000000', '320205000000', '锡山区');
INSERT INTO `j_position_county` VALUES ('746', '320200000000', '320206000000', '惠山区');
INSERT INTO `j_position_county` VALUES ('747', '320200000000', '320211000000', '滨湖区');
INSERT INTO `j_position_county` VALUES ('748', '320200000000', '320281000000', '江阴市');
INSERT INTO `j_position_county` VALUES ('749', '320200000000', '320282000000', '宜兴市');
INSERT INTO `j_position_county` VALUES ('750', '320300000000', '320302000000', '鼓楼区');
INSERT INTO `j_position_county` VALUES ('751', '320300000000', '320303000000', '云龙区');
INSERT INTO `j_position_county` VALUES ('752', '320300000000', '320305000000', '贾汪区');
INSERT INTO `j_position_county` VALUES ('753', '320300000000', '320311000000', '泉山区');
INSERT INTO `j_position_county` VALUES ('754', '320300000000', '320312000000', '铜山区');
INSERT INTO `j_position_county` VALUES ('755', '320300000000', '320321000000', '丰县');
INSERT INTO `j_position_county` VALUES ('756', '320300000000', '320322000000', '沛县');
INSERT INTO `j_position_county` VALUES ('757', '320300000000', '320324000000', '睢宁县');
INSERT INTO `j_position_county` VALUES ('758', '320300000000', '320381000000', '新沂市');
INSERT INTO `j_position_county` VALUES ('759', '320300000000', '320382000000', '邳州市');
INSERT INTO `j_position_county` VALUES ('760', '320400000000', '320402000000', '天宁区');
INSERT INTO `j_position_county` VALUES ('761', '320400000000', '320404000000', '钟楼区');
INSERT INTO `j_position_county` VALUES ('762', '320400000000', '320405000000', '戚墅堰区');
INSERT INTO `j_position_county` VALUES ('763', '320400000000', '320411000000', '新北区');
INSERT INTO `j_position_county` VALUES ('764', '320400000000', '320412000000', '武进区');
INSERT INTO `j_position_county` VALUES ('765', '320400000000', '320481000000', '溧阳市');
INSERT INTO `j_position_county` VALUES ('766', '320400000000', '320482000000', '金坛市');
INSERT INTO `j_position_county` VALUES ('767', '320500000000', '320505000000', '虎丘区');
INSERT INTO `j_position_county` VALUES ('768', '320500000000', '320506000000', '吴中区');
INSERT INTO `j_position_county` VALUES ('769', '320500000000', '320507000000', '相城区');
INSERT INTO `j_position_county` VALUES ('770', '320500000000', '320508000000', '姑苏区');
INSERT INTO `j_position_county` VALUES ('771', '320500000000', '320509000000', '吴江区');
INSERT INTO `j_position_county` VALUES ('772', '320500000000', '320581000000', '常熟市');
INSERT INTO `j_position_county` VALUES ('773', '320500000000', '320582000000', '张家港市');
INSERT INTO `j_position_county` VALUES ('774', '320500000000', '320583000000', '昆山市');
INSERT INTO `j_position_county` VALUES ('775', '320500000000', '320585000000', '太仓市');
INSERT INTO `j_position_county` VALUES ('776', '320600000000', '320602000000', '崇川区');
INSERT INTO `j_position_county` VALUES ('777', '320600000000', '320611000000', '港闸区');
INSERT INTO `j_position_county` VALUES ('778', '320600000000', '320612000000', '通州区');
INSERT INTO `j_position_county` VALUES ('779', '320600000000', '320621000000', '海安县');
INSERT INTO `j_position_county` VALUES ('780', '320600000000', '320623000000', '如东县');
INSERT INTO `j_position_county` VALUES ('781', '320600000000', '320681000000', '启东市');
INSERT INTO `j_position_county` VALUES ('782', '320600000000', '320682000000', '如皋市');
INSERT INTO `j_position_county` VALUES ('783', '320600000000', '320684000000', '海门市');
INSERT INTO `j_position_county` VALUES ('784', '320700000000', '320703000000', '连云区');
INSERT INTO `j_position_county` VALUES ('785', '320700000000', '320705000000', '新浦区');
INSERT INTO `j_position_county` VALUES ('786', '320700000000', '320706000000', '海州区');
INSERT INTO `j_position_county` VALUES ('787', '320700000000', '320721000000', '赣榆县');
INSERT INTO `j_position_county` VALUES ('788', '320700000000', '320722000000', '东海县');
INSERT INTO `j_position_county` VALUES ('789', '320700000000', '320723000000', '灌云县');
INSERT INTO `j_position_county` VALUES ('790', '320700000000', '320724000000', '灌南县');
INSERT INTO `j_position_county` VALUES ('791', '320800000000', '320802000000', '清河区');
INSERT INTO `j_position_county` VALUES ('792', '320800000000', '320803000000', '淮安区');
INSERT INTO `j_position_county` VALUES ('793', '320800000000', '320804000000', '淮阴区');
INSERT INTO `j_position_county` VALUES ('794', '320800000000', '320811000000', '清浦区');
INSERT INTO `j_position_county` VALUES ('795', '320800000000', '320826000000', '涟水县');
INSERT INTO `j_position_county` VALUES ('796', '320800000000', '320829000000', '洪泽县');
INSERT INTO `j_position_county` VALUES ('797', '320800000000', '320830000000', '盱眙县');
INSERT INTO `j_position_county` VALUES ('798', '320800000000', '320831000000', '金湖县');
INSERT INTO `j_position_county` VALUES ('799', '320900000000', '320902000000', '亭湖区');
INSERT INTO `j_position_county` VALUES ('800', '320900000000', '320903000000', '盐都区');
INSERT INTO `j_position_county` VALUES ('801', '320900000000', '320921000000', '响水县');
INSERT INTO `j_position_county` VALUES ('802', '320900000000', '320922000000', '滨海县');
INSERT INTO `j_position_county` VALUES ('803', '320900000000', '320923000000', '阜宁县');
INSERT INTO `j_position_county` VALUES ('804', '320900000000', '320924000000', '射阳县');
INSERT INTO `j_position_county` VALUES ('805', '320900000000', '320925000000', '建湖县');
INSERT INTO `j_position_county` VALUES ('806', '320900000000', '320981000000', '东台市');
INSERT INTO `j_position_county` VALUES ('807', '320900000000', '320982000000', '大丰市');
INSERT INTO `j_position_county` VALUES ('808', '321000000000', '321002000000', '广陵区');
INSERT INTO `j_position_county` VALUES ('809', '321000000000', '321003000000', '邗江区');
INSERT INTO `j_position_county` VALUES ('810', '321000000000', '321012000000', '江都区');
INSERT INTO `j_position_county` VALUES ('811', '321000000000', '321023000000', '宝应县');
INSERT INTO `j_position_county` VALUES ('812', '321000000000', '321081000000', '仪征市');
INSERT INTO `j_position_county` VALUES ('813', '321000000000', '321084000000', '高邮市');
INSERT INTO `j_position_county` VALUES ('814', '321100000000', '321102000000', '京口区');
INSERT INTO `j_position_county` VALUES ('815', '321100000000', '321111000000', '润州区');
INSERT INTO `j_position_county` VALUES ('816', '321100000000', '321112000000', '丹徒区');
INSERT INTO `j_position_county` VALUES ('817', '321100000000', '321181000000', '丹阳市');
INSERT INTO `j_position_county` VALUES ('818', '321100000000', '321182000000', '扬中市');
INSERT INTO `j_position_county` VALUES ('819', '321100000000', '321183000000', '句容市');
INSERT INTO `j_position_county` VALUES ('820', '321200000000', '321202000000', '海陵区');
INSERT INTO `j_position_county` VALUES ('821', '321200000000', '321203000000', '高港区');
INSERT INTO `j_position_county` VALUES ('822', '321200000000', '321204000000', '姜堰区');
INSERT INTO `j_position_county` VALUES ('823', '321200000000', '321281000000', '兴化市');
INSERT INTO `j_position_county` VALUES ('824', '321200000000', '321282000000', '靖江市');
INSERT INTO `j_position_county` VALUES ('825', '321200000000', '321283000000', '泰兴市');
INSERT INTO `j_position_county` VALUES ('826', '321300000000', '321302000000', '宿城区');
INSERT INTO `j_position_county` VALUES ('827', '321300000000', '321311000000', '宿豫区');
INSERT INTO `j_position_county` VALUES ('828', '321300000000', '321322000000', '沭阳县');
INSERT INTO `j_position_county` VALUES ('829', '321300000000', '321323000000', '泗阳县');
INSERT INTO `j_position_county` VALUES ('830', '321300000000', '321324000000', '泗洪县');
INSERT INTO `j_position_county` VALUES ('831', '330100000000', '330102000000', '上城区');
INSERT INTO `j_position_county` VALUES ('832', '330100000000', '330103000000', '下城区');
INSERT INTO `j_position_county` VALUES ('833', '330100000000', '330104000000', '江干区');
INSERT INTO `j_position_county` VALUES ('834', '330100000000', '330105000000', '拱墅区');
INSERT INTO `j_position_county` VALUES ('835', '330100000000', '330106000000', '西湖区');
INSERT INTO `j_position_county` VALUES ('836', '330100000000', '330108000000', '滨江区');
INSERT INTO `j_position_county` VALUES ('837', '330100000000', '330109000000', '萧山区');
INSERT INTO `j_position_county` VALUES ('838', '330100000000', '330110000000', '余杭区');
INSERT INTO `j_position_county` VALUES ('839', '330100000000', '330122000000', '桐庐县');
INSERT INTO `j_position_county` VALUES ('840', '330100000000', '330127000000', '淳安县');
INSERT INTO `j_position_county` VALUES ('841', '330100000000', '330182000000', '建德市');
INSERT INTO `j_position_county` VALUES ('842', '330100000000', '330183000000', '富阳市');
INSERT INTO `j_position_county` VALUES ('843', '330100000000', '330185000000', '临安市');
INSERT INTO `j_position_county` VALUES ('844', '330200000000', '330203000000', '海曙区');
INSERT INTO `j_position_county` VALUES ('845', '330200000000', '330204000000', '江东区');
INSERT INTO `j_position_county` VALUES ('846', '330200000000', '330205000000', '江北区');
INSERT INTO `j_position_county` VALUES ('847', '330200000000', '330206000000', '北仑区');
INSERT INTO `j_position_county` VALUES ('848', '330200000000', '330211000000', '镇海区');
INSERT INTO `j_position_county` VALUES ('849', '330200000000', '330212000000', '鄞州区');
INSERT INTO `j_position_county` VALUES ('850', '330200000000', '330225000000', '象山县');
INSERT INTO `j_position_county` VALUES ('851', '330200000000', '330226000000', '宁海县');
INSERT INTO `j_position_county` VALUES ('852', '330200000000', '330281000000', '余姚市');
INSERT INTO `j_position_county` VALUES ('853', '330200000000', '330282000000', '慈溪市');
INSERT INTO `j_position_county` VALUES ('854', '330200000000', '330283000000', '奉化市');
INSERT INTO `j_position_county` VALUES ('855', '330300000000', '330302000000', '鹿城区');
INSERT INTO `j_position_county` VALUES ('856', '330300000000', '330303000000', '龙湾区');
INSERT INTO `j_position_county` VALUES ('857', '330300000000', '330304000000', '瓯海区');
INSERT INTO `j_position_county` VALUES ('858', '330300000000', '330322000000', '洞头县');
INSERT INTO `j_position_county` VALUES ('859', '330300000000', '330324000000', '永嘉县');
INSERT INTO `j_position_county` VALUES ('860', '330300000000', '330326000000', '平阳县');
INSERT INTO `j_position_county` VALUES ('861', '330300000000', '330327000000', '苍南县');
INSERT INTO `j_position_county` VALUES ('862', '330300000000', '330328000000', '文成县');
INSERT INTO `j_position_county` VALUES ('863', '330300000000', '330329000000', '泰顺县');
INSERT INTO `j_position_county` VALUES ('864', '330300000000', '330381000000', '瑞安市');
INSERT INTO `j_position_county` VALUES ('865', '330300000000', '330382000000', '乐清市');
INSERT INTO `j_position_county` VALUES ('866', '330400000000', '330402000000', '南湖区');
INSERT INTO `j_position_county` VALUES ('867', '330400000000', '330411000000', '秀洲区');
INSERT INTO `j_position_county` VALUES ('868', '330400000000', '330421000000', '嘉善县');
INSERT INTO `j_position_county` VALUES ('869', '330400000000', '330424000000', '海盐县');
INSERT INTO `j_position_county` VALUES ('870', '330400000000', '330481000000', '海宁市');
INSERT INTO `j_position_county` VALUES ('871', '330400000000', '330482000000', '平湖市');
INSERT INTO `j_position_county` VALUES ('872', '330400000000', '330483000000', '桐乡市');
INSERT INTO `j_position_county` VALUES ('873', '330500000000', '330502000000', '吴兴区');
INSERT INTO `j_position_county` VALUES ('874', '330500000000', '330503000000', '南浔区');
INSERT INTO `j_position_county` VALUES ('875', '330500000000', '330521000000', '德清县');
INSERT INTO `j_position_county` VALUES ('876', '330500000000', '330522000000', '长兴县');
INSERT INTO `j_position_county` VALUES ('877', '330500000000', '330523000000', '安吉县');
INSERT INTO `j_position_county` VALUES ('878', '330600000000', '330602000000', '越城区');
INSERT INTO `j_position_county` VALUES ('879', '330600000000', '330621000000', '绍兴县');
INSERT INTO `j_position_county` VALUES ('880', '330600000000', '330624000000', '新昌县');
INSERT INTO `j_position_county` VALUES ('881', '330600000000', '330681000000', '诸暨市');
INSERT INTO `j_position_county` VALUES ('882', '330600000000', '330682000000', '上虞市');
INSERT INTO `j_position_county` VALUES ('883', '330600000000', '330683000000', '嵊州市');
INSERT INTO `j_position_county` VALUES ('884', '330700000000', '330702000000', '婺城区');
INSERT INTO `j_position_county` VALUES ('885', '330700000000', '330703000000', '金东区');
INSERT INTO `j_position_county` VALUES ('886', '330700000000', '330723000000', '武义县');
INSERT INTO `j_position_county` VALUES ('887', '330700000000', '330726000000', '浦江县');
INSERT INTO `j_position_county` VALUES ('888', '330700000000', '330727000000', '磐安县');
INSERT INTO `j_position_county` VALUES ('889', '330700000000', '330781000000', '兰溪市');
INSERT INTO `j_position_county` VALUES ('890', '330700000000', '330782000000', '义乌市');
INSERT INTO `j_position_county` VALUES ('891', '330700000000', '330783000000', '东阳市');
INSERT INTO `j_position_county` VALUES ('892', '330700000000', '330784000000', '永康市');
INSERT INTO `j_position_county` VALUES ('893', '330800000000', '330802000000', '柯城区');
INSERT INTO `j_position_county` VALUES ('894', '330800000000', '330803000000', '衢江区');
INSERT INTO `j_position_county` VALUES ('895', '330800000000', '330822000000', '常山县');
INSERT INTO `j_position_county` VALUES ('896', '330800000000', '330824000000', '开化县');
INSERT INTO `j_position_county` VALUES ('897', '330800000000', '330825000000', '龙游县');
INSERT INTO `j_position_county` VALUES ('898', '330800000000', '330881000000', '江山市');
INSERT INTO `j_position_county` VALUES ('899', '330900000000', '330902000000', '定海区');
INSERT INTO `j_position_county` VALUES ('900', '330900000000', '330903000000', '普陀区');
INSERT INTO `j_position_county` VALUES ('901', '330900000000', '330921000000', '岱山县');
INSERT INTO `j_position_county` VALUES ('902', '330900000000', '330922000000', '嵊泗县');
INSERT INTO `j_position_county` VALUES ('903', '331000000000', '331002000000', '椒江区');
INSERT INTO `j_position_county` VALUES ('904', '331000000000', '331003000000', '黄岩区');
INSERT INTO `j_position_county` VALUES ('905', '331000000000', '331004000000', '路桥区');
INSERT INTO `j_position_county` VALUES ('906', '331000000000', '331021000000', '玉环县');
INSERT INTO `j_position_county` VALUES ('907', '331000000000', '331022000000', '三门县');
INSERT INTO `j_position_county` VALUES ('908', '331000000000', '331023000000', '天台县');
INSERT INTO `j_position_county` VALUES ('909', '331000000000', '331024000000', '仙居县');
INSERT INTO `j_position_county` VALUES ('910', '331000000000', '331081000000', '温岭市');
INSERT INTO `j_position_county` VALUES ('911', '331000000000', '331082000000', '临海市');
INSERT INTO `j_position_county` VALUES ('912', '331100000000', '331102000000', '莲都区');
INSERT INTO `j_position_county` VALUES ('913', '331100000000', '331121000000', '青田县');
INSERT INTO `j_position_county` VALUES ('914', '331100000000', '331122000000', '缙云县');
INSERT INTO `j_position_county` VALUES ('915', '331100000000', '331123000000', '遂昌县');
INSERT INTO `j_position_county` VALUES ('916', '331100000000', '331124000000', '松阳县');
INSERT INTO `j_position_county` VALUES ('917', '331100000000', '331125000000', '云和县');
INSERT INTO `j_position_county` VALUES ('918', '331100000000', '331126000000', '庆元县');
INSERT INTO `j_position_county` VALUES ('919', '331100000000', '331127000000', '景宁畲族自治县');
INSERT INTO `j_position_county` VALUES ('920', '331100000000', '331181000000', '龙泉市');
INSERT INTO `j_position_county` VALUES ('921', '340100000000', '340102000000', '瑶海区');
INSERT INTO `j_position_county` VALUES ('922', '340100000000', '340103000000', '庐阳区');
INSERT INTO `j_position_county` VALUES ('923', '340100000000', '340104000000', '蜀山区');
INSERT INTO `j_position_county` VALUES ('924', '340100000000', '340111000000', '包河区');
INSERT INTO `j_position_county` VALUES ('925', '340100000000', '340121000000', '长丰县');
INSERT INTO `j_position_county` VALUES ('926', '340100000000', '340122000000', '肥东县');
INSERT INTO `j_position_county` VALUES ('927', '340100000000', '340123000000', '肥西县');
INSERT INTO `j_position_county` VALUES ('928', '340100000000', '340124000000', '庐江县');
INSERT INTO `j_position_county` VALUES ('929', '340100000000', '340181000000', '巢湖市');
INSERT INTO `j_position_county` VALUES ('930', '340200000000', '340202000000', '镜湖区');
INSERT INTO `j_position_county` VALUES ('931', '340200000000', '340203000000', '弋江区');
INSERT INTO `j_position_county` VALUES ('932', '340200000000', '340207000000', '鸠江区');
INSERT INTO `j_position_county` VALUES ('933', '340200000000', '340208000000', '三山区');
INSERT INTO `j_position_county` VALUES ('934', '340200000000', '340221000000', '芜湖县');
INSERT INTO `j_position_county` VALUES ('935', '340200000000', '340222000000', '繁昌县');
INSERT INTO `j_position_county` VALUES ('936', '340200000000', '340223000000', '南陵县');
INSERT INTO `j_position_county` VALUES ('937', '340200000000', '340225000000', '无为县');
INSERT INTO `j_position_county` VALUES ('938', '340300000000', '340302000000', '龙子湖区');
INSERT INTO `j_position_county` VALUES ('939', '340300000000', '340303000000', '蚌山区');
INSERT INTO `j_position_county` VALUES ('940', '340300000000', '340304000000', '禹会区');
INSERT INTO `j_position_county` VALUES ('941', '340300000000', '340311000000', '淮上区');
INSERT INTO `j_position_county` VALUES ('942', '340300000000', '340321000000', '怀远县');
INSERT INTO `j_position_county` VALUES ('943', '340300000000', '340322000000', '五河县');
INSERT INTO `j_position_county` VALUES ('944', '340300000000', '340323000000', '固镇县');
INSERT INTO `j_position_county` VALUES ('945', '340400000000', '340402000000', '大通区');
INSERT INTO `j_position_county` VALUES ('946', '340400000000', '340403000000', '田家庵区');
INSERT INTO `j_position_county` VALUES ('947', '340400000000', '340404000000', '谢家集区');
INSERT INTO `j_position_county` VALUES ('948', '340400000000', '340405000000', '八公山区');
INSERT INTO `j_position_county` VALUES ('949', '340400000000', '340406000000', '潘集区');
INSERT INTO `j_position_county` VALUES ('950', '340400000000', '340421000000', '凤台县');
INSERT INTO `j_position_county` VALUES ('951', '340500000000', '340503000000', '花山区');
INSERT INTO `j_position_county` VALUES ('952', '340500000000', '340504000000', '雨山区');
INSERT INTO `j_position_county` VALUES ('953', '340500000000', '340506000000', '博望区');
INSERT INTO `j_position_county` VALUES ('954', '340500000000', '340521000000', '当涂县');
INSERT INTO `j_position_county` VALUES ('955', '340500000000', '340522000000', '含山县');
INSERT INTO `j_position_county` VALUES ('956', '340500000000', '340523000000', '和县');
INSERT INTO `j_position_county` VALUES ('957', '340600000000', '340602000000', '杜集区');
INSERT INTO `j_position_county` VALUES ('958', '340600000000', '340603000000', '相山区');
INSERT INTO `j_position_county` VALUES ('959', '340600000000', '340604000000', '烈山区');
INSERT INTO `j_position_county` VALUES ('960', '340600000000', '340621000000', '濉溪县');
INSERT INTO `j_position_county` VALUES ('961', '340700000000', '340702000000', '铜官山区');
INSERT INTO `j_position_county` VALUES ('962', '340700000000', '340703000000', '狮子山区');
INSERT INTO `j_position_county` VALUES ('963', '340700000000', '340711000000', '郊区');
INSERT INTO `j_position_county` VALUES ('964', '340700000000', '340721000000', '铜陵县');
INSERT INTO `j_position_county` VALUES ('965', '340800000000', '340802000000', '迎江区');
INSERT INTO `j_position_county` VALUES ('966', '340800000000', '340803000000', '大观区');
INSERT INTO `j_position_county` VALUES ('967', '340800000000', '340811000000', '宜秀区');
INSERT INTO `j_position_county` VALUES ('968', '340800000000', '340822000000', '怀宁县');
INSERT INTO `j_position_county` VALUES ('969', '340800000000', '340823000000', '枞阳县');
INSERT INTO `j_position_county` VALUES ('970', '340800000000', '340824000000', '潜山县');
INSERT INTO `j_position_county` VALUES ('971', '340800000000', '340825000000', '太湖县');
INSERT INTO `j_position_county` VALUES ('972', '340800000000', '340826000000', '宿松县');
INSERT INTO `j_position_county` VALUES ('973', '340800000000', '340827000000', '望江县');
INSERT INTO `j_position_county` VALUES ('974', '340800000000', '340828000000', '岳西县');
INSERT INTO `j_position_county` VALUES ('975', '340800000000', '340881000000', '桐城市');
INSERT INTO `j_position_county` VALUES ('976', '341000000000', '341002000000', '屯溪区');
INSERT INTO `j_position_county` VALUES ('977', '341000000000', '341003000000', '黄山区');
INSERT INTO `j_position_county` VALUES ('978', '341000000000', '341004000000', '徽州区');
INSERT INTO `j_position_county` VALUES ('979', '341000000000', '341021000000', '歙县');
INSERT INTO `j_position_county` VALUES ('980', '341000000000', '341022000000', '休宁县');
INSERT INTO `j_position_county` VALUES ('981', '341000000000', '341023000000', '黟县');
INSERT INTO `j_position_county` VALUES ('982', '341000000000', '341024000000', '祁门县');
INSERT INTO `j_position_county` VALUES ('983', '341100000000', '341102000000', '琅琊区');
INSERT INTO `j_position_county` VALUES ('984', '341100000000', '341103000000', '南谯区');
INSERT INTO `j_position_county` VALUES ('985', '341100000000', '341122000000', '来安县');
INSERT INTO `j_position_county` VALUES ('986', '341100000000', '341124000000', '全椒县');
INSERT INTO `j_position_county` VALUES ('987', '341100000000', '341125000000', '定远县');
INSERT INTO `j_position_county` VALUES ('988', '341100000000', '341126000000', '凤阳县');
INSERT INTO `j_position_county` VALUES ('989', '341100000000', '341181000000', '天长市');
INSERT INTO `j_position_county` VALUES ('990', '341100000000', '341182000000', '明光市');
INSERT INTO `j_position_county` VALUES ('991', '341200000000', '341202000000', '颍州区');
INSERT INTO `j_position_county` VALUES ('992', '341200000000', '341203000000', '颍东区');
INSERT INTO `j_position_county` VALUES ('993', '341200000000', '341204000000', '颍泉区');
INSERT INTO `j_position_county` VALUES ('994', '341200000000', '341221000000', '临泉县');
INSERT INTO `j_position_county` VALUES ('995', '341200000000', '341222000000', '太和县');
INSERT INTO `j_position_county` VALUES ('996', '341200000000', '341225000000', '阜南县');
INSERT INTO `j_position_county` VALUES ('997', '341200000000', '341226000000', '颍上县');
INSERT INTO `j_position_county` VALUES ('998', '341200000000', '341282000000', '界首市');
INSERT INTO `j_position_county` VALUES ('999', '341300000000', '341302000000', '埇桥区');
INSERT INTO `j_position_county` VALUES ('1000', '341300000000', '341321000000', '砀山县');
INSERT INTO `j_position_county` VALUES ('1001', '341300000000', '341322000000', '萧县');
INSERT INTO `j_position_county` VALUES ('1002', '341300000000', '341323000000', '灵璧县');
INSERT INTO `j_position_county` VALUES ('1003', '341300000000', '341324000000', '泗县');
INSERT INTO `j_position_county` VALUES ('1004', '341500000000', '341502000000', '金安区');
INSERT INTO `j_position_county` VALUES ('1005', '341500000000', '341503000000', '裕安区');
INSERT INTO `j_position_county` VALUES ('1006', '341500000000', '341521000000', '寿县');
INSERT INTO `j_position_county` VALUES ('1007', '341500000000', '341522000000', '霍邱县');
INSERT INTO `j_position_county` VALUES ('1008', '341500000000', '341523000000', '舒城县');
INSERT INTO `j_position_county` VALUES ('1009', '341500000000', '341524000000', '金寨县');
INSERT INTO `j_position_county` VALUES ('1010', '341500000000', '341525000000', '霍山县');
INSERT INTO `j_position_county` VALUES ('1011', '341600000000', '341602000000', '谯城区');
INSERT INTO `j_position_county` VALUES ('1012', '341600000000', '341621000000', '涡阳县');
INSERT INTO `j_position_county` VALUES ('1013', '341600000000', '341622000000', '蒙城县');
INSERT INTO `j_position_county` VALUES ('1014', '341600000000', '341623000000', '利辛县');
INSERT INTO `j_position_county` VALUES ('1015', '341700000000', '341702000000', '贵池区');
INSERT INTO `j_position_county` VALUES ('1016', '341700000000', '341721000000', '东至县');
INSERT INTO `j_position_county` VALUES ('1017', '341700000000', '341722000000', '石台县');
INSERT INTO `j_position_county` VALUES ('1018', '341700000000', '341723000000', '青阳县');
INSERT INTO `j_position_county` VALUES ('1019', '341800000000', '341802000000', '宣州区');
INSERT INTO `j_position_county` VALUES ('1020', '341800000000', '341821000000', '郎溪县');
INSERT INTO `j_position_county` VALUES ('1021', '341800000000', '341822000000', '广德县');
INSERT INTO `j_position_county` VALUES ('1022', '341800000000', '341823000000', '泾县');
INSERT INTO `j_position_county` VALUES ('1023', '341800000000', '341824000000', '绩溪县');
INSERT INTO `j_position_county` VALUES ('1024', '341800000000', '341825000000', '旌德县');
INSERT INTO `j_position_county` VALUES ('1025', '341800000000', '341881000000', '宁国市');
INSERT INTO `j_position_county` VALUES ('1026', '350100000000', '350102000000', '鼓楼区');
INSERT INTO `j_position_county` VALUES ('1027', '350100000000', '350103000000', '台江区');
INSERT INTO `j_position_county` VALUES ('1028', '350100000000', '350104000000', '仓山区');
INSERT INTO `j_position_county` VALUES ('1029', '350100000000', '350105000000', '马尾区');
INSERT INTO `j_position_county` VALUES ('1030', '350100000000', '350111000000', '晋安区');
INSERT INTO `j_position_county` VALUES ('1031', '350100000000', '350121000000', '闽侯县');
INSERT INTO `j_position_county` VALUES ('1032', '350100000000', '350122000000', '连江县');
INSERT INTO `j_position_county` VALUES ('1033', '350100000000', '350123000000', '罗源县');
INSERT INTO `j_position_county` VALUES ('1034', '350100000000', '350124000000', '闽清县');
INSERT INTO `j_position_county` VALUES ('1035', '350100000000', '350125000000', '永泰县');
INSERT INTO `j_position_county` VALUES ('1036', '350100000000', '350128000000', '平潭县');
INSERT INTO `j_position_county` VALUES ('1037', '350100000000', '350181000000', '福清市');
INSERT INTO `j_position_county` VALUES ('1038', '350100000000', '350182000000', '长乐市');
INSERT INTO `j_position_county` VALUES ('1039', '350200000000', '350203000000', '思明区');
INSERT INTO `j_position_county` VALUES ('1040', '350200000000', '350205000000', '海沧区');
INSERT INTO `j_position_county` VALUES ('1041', '350200000000', '350206000000', '湖里区');
INSERT INTO `j_position_county` VALUES ('1042', '350200000000', '350211000000', '集美区');
INSERT INTO `j_position_county` VALUES ('1043', '350200000000', '350212000000', '同安区');
INSERT INTO `j_position_county` VALUES ('1044', '350200000000', '350213000000', '翔安区');
INSERT INTO `j_position_county` VALUES ('1045', '350300000000', '350302000000', '城厢区');
INSERT INTO `j_position_county` VALUES ('1046', '350300000000', '350303000000', '涵江区');
INSERT INTO `j_position_county` VALUES ('1047', '350300000000', '350304000000', '荔城区');
INSERT INTO `j_position_county` VALUES ('1048', '350300000000', '350305000000', '秀屿区');
INSERT INTO `j_position_county` VALUES ('1049', '350300000000', '350322000000', '仙游县');
INSERT INTO `j_position_county` VALUES ('1050', '350400000000', '350402000000', '梅列区');
INSERT INTO `j_position_county` VALUES ('1051', '350400000000', '350403000000', '三元区');
INSERT INTO `j_position_county` VALUES ('1052', '350400000000', '350421000000', '明溪县');
INSERT INTO `j_position_county` VALUES ('1053', '350400000000', '350423000000', '清流县');
INSERT INTO `j_position_county` VALUES ('1054', '350400000000', '350424000000', '宁化县');
INSERT INTO `j_position_county` VALUES ('1055', '350400000000', '350425000000', '大田县');
INSERT INTO `j_position_county` VALUES ('1056', '350400000000', '350426000000', '尤溪县');
INSERT INTO `j_position_county` VALUES ('1057', '350400000000', '350427000000', '沙县');
INSERT INTO `j_position_county` VALUES ('1058', '350400000000', '350428000000', '将乐县');
INSERT INTO `j_position_county` VALUES ('1059', '350400000000', '350429000000', '泰宁县');
INSERT INTO `j_position_county` VALUES ('1060', '350400000000', '350430000000', '建宁县');
INSERT INTO `j_position_county` VALUES ('1061', '350400000000', '350481000000', '永安市');
INSERT INTO `j_position_county` VALUES ('1062', '350500000000', '350502000000', '鲤城区');
INSERT INTO `j_position_county` VALUES ('1063', '350500000000', '350503000000', '丰泽区');
INSERT INTO `j_position_county` VALUES ('1064', '350500000000', '350504000000', '洛江区');
INSERT INTO `j_position_county` VALUES ('1065', '350500000000', '350505000000', '泉港区');
INSERT INTO `j_position_county` VALUES ('1066', '350500000000', '350521000000', '惠安县');
INSERT INTO `j_position_county` VALUES ('1067', '350500000000', '350524000000', '安溪县');
INSERT INTO `j_position_county` VALUES ('1068', '350500000000', '350525000000', '永春县');
INSERT INTO `j_position_county` VALUES ('1069', '350500000000', '350526000000', '德化县');
INSERT INTO `j_position_county` VALUES ('1070', '350500000000', '350581000000', '石狮市');
INSERT INTO `j_position_county` VALUES ('1071', '350500000000', '350582000000', '晋江市');
INSERT INTO `j_position_county` VALUES ('1072', '350500000000', '350583000000', '南安市');
INSERT INTO `j_position_county` VALUES ('1073', '350600000000', '350602000000', '芗城区');
INSERT INTO `j_position_county` VALUES ('1074', '350600000000', '350603000000', '龙文区');
INSERT INTO `j_position_county` VALUES ('1075', '350600000000', '350622000000', '云霄县');
INSERT INTO `j_position_county` VALUES ('1076', '350600000000', '350623000000', '漳浦县');
INSERT INTO `j_position_county` VALUES ('1077', '350600000000', '350624000000', '诏安县');
INSERT INTO `j_position_county` VALUES ('1078', '350600000000', '350625000000', '长泰县');
INSERT INTO `j_position_county` VALUES ('1079', '350600000000', '350626000000', '东山县');
INSERT INTO `j_position_county` VALUES ('1080', '350600000000', '350627000000', '南靖县');
INSERT INTO `j_position_county` VALUES ('1081', '350600000000', '350628000000', '平和县');
INSERT INTO `j_position_county` VALUES ('1082', '350600000000', '350629000000', '华安县');
INSERT INTO `j_position_county` VALUES ('1083', '350600000000', '350681000000', '龙海市');
INSERT INTO `j_position_county` VALUES ('1084', '350700000000', '350702000000', '延平区');
INSERT INTO `j_position_county` VALUES ('1085', '350700000000', '350721000000', '顺昌县');
INSERT INTO `j_position_county` VALUES ('1086', '350700000000', '350722000000', '浦城县');
INSERT INTO `j_position_county` VALUES ('1087', '350700000000', '350723000000', '光泽县');
INSERT INTO `j_position_county` VALUES ('1088', '350700000000', '350724000000', '松溪县');
INSERT INTO `j_position_county` VALUES ('1089', '350700000000', '350725000000', '政和县');
INSERT INTO `j_position_county` VALUES ('1090', '350700000000', '350781000000', '邵武市');
INSERT INTO `j_position_county` VALUES ('1091', '350700000000', '350782000000', '武夷山市');
INSERT INTO `j_position_county` VALUES ('1092', '350700000000', '350783000000', '建瓯市');
INSERT INTO `j_position_county` VALUES ('1093', '350700000000', '350784000000', '建阳市');
INSERT INTO `j_position_county` VALUES ('1094', '350800000000', '350802000000', '新罗区');
INSERT INTO `j_position_county` VALUES ('1095', '350800000000', '350821000000', '长汀县');
INSERT INTO `j_position_county` VALUES ('1096', '350800000000', '350822000000', '永定县');
INSERT INTO `j_position_county` VALUES ('1097', '350800000000', '350823000000', '上杭县');
INSERT INTO `j_position_county` VALUES ('1098', '350800000000', '350824000000', '武平县');
INSERT INTO `j_position_county` VALUES ('1099', '350800000000', '350825000000', '连城县');
INSERT INTO `j_position_county` VALUES ('1100', '350800000000', '350881000000', '漳平市');
INSERT INTO `j_position_county` VALUES ('1101', '350900000000', '350902000000', '蕉城区');
INSERT INTO `j_position_county` VALUES ('1102', '350900000000', '350921000000', '霞浦县');
INSERT INTO `j_position_county` VALUES ('1103', '350900000000', '350922000000', '古田县');
INSERT INTO `j_position_county` VALUES ('1104', '350900000000', '350923000000', '屏南县');
INSERT INTO `j_position_county` VALUES ('1105', '350900000000', '350924000000', '寿宁县');
INSERT INTO `j_position_county` VALUES ('1106', '350900000000', '350925000000', '周宁县');
INSERT INTO `j_position_county` VALUES ('1107', '350900000000', '350926000000', '柘荣县');
INSERT INTO `j_position_county` VALUES ('1108', '350900000000', '350981000000', '福安市');
INSERT INTO `j_position_county` VALUES ('1109', '350900000000', '350982000000', '福鼎市');
INSERT INTO `j_position_county` VALUES ('1110', '360100000000', '360102000000', '东湖区');
INSERT INTO `j_position_county` VALUES ('1111', '360100000000', '360103000000', '西湖区');
INSERT INTO `j_position_county` VALUES ('1112', '360100000000', '360104000000', '青云谱区');
INSERT INTO `j_position_county` VALUES ('1113', '360100000000', '360105000000', '湾里区');
INSERT INTO `j_position_county` VALUES ('1114', '360100000000', '360111000000', '青山湖区');
INSERT INTO `j_position_county` VALUES ('1115', '360100000000', '360121000000', '南昌县');
INSERT INTO `j_position_county` VALUES ('1116', '360100000000', '360122000000', '新建县');
INSERT INTO `j_position_county` VALUES ('1117', '360100000000', '360123000000', '安义县');
INSERT INTO `j_position_county` VALUES ('1118', '360100000000', '360124000000', '进贤县');
INSERT INTO `j_position_county` VALUES ('1119', '360200000000', '360202000000', '昌江区');
INSERT INTO `j_position_county` VALUES ('1120', '360200000000', '360203000000', '珠山区');
INSERT INTO `j_position_county` VALUES ('1121', '360200000000', '360222000000', '浮梁县');
INSERT INTO `j_position_county` VALUES ('1122', '360200000000', '360281000000', '乐平市');
INSERT INTO `j_position_county` VALUES ('1123', '360300000000', '360302000000', '安源区');
INSERT INTO `j_position_county` VALUES ('1124', '360300000000', '360313000000', '湘东区');
INSERT INTO `j_position_county` VALUES ('1125', '360300000000', '360321000000', '莲花县');
INSERT INTO `j_position_county` VALUES ('1126', '360300000000', '360322000000', '上栗县');
INSERT INTO `j_position_county` VALUES ('1127', '360300000000', '360323000000', '芦溪县');
INSERT INTO `j_position_county` VALUES ('1128', '360400000000', '360402000000', '庐山区');
INSERT INTO `j_position_county` VALUES ('1129', '360400000000', '360403000000', '浔阳区');
INSERT INTO `j_position_county` VALUES ('1130', '360400000000', '360421000000', '九江县');
INSERT INTO `j_position_county` VALUES ('1131', '360400000000', '360423000000', '武宁县');
INSERT INTO `j_position_county` VALUES ('1132', '360400000000', '360424000000', '修水县');
INSERT INTO `j_position_county` VALUES ('1133', '360400000000', '360425000000', '永修县');
INSERT INTO `j_position_county` VALUES ('1134', '360400000000', '360426000000', '德安县');
INSERT INTO `j_position_county` VALUES ('1135', '360400000000', '360427000000', '星子县');
INSERT INTO `j_position_county` VALUES ('1136', '360400000000', '360428000000', '都昌县');
INSERT INTO `j_position_county` VALUES ('1137', '360400000000', '360429000000', '湖口县');
INSERT INTO `j_position_county` VALUES ('1138', '360400000000', '360430000000', '彭泽县');
INSERT INTO `j_position_county` VALUES ('1139', '360400000000', '360481000000', '瑞昌市');
INSERT INTO `j_position_county` VALUES ('1140', '360400000000', '360482000000', '共青城市');
INSERT INTO `j_position_county` VALUES ('1141', '360500000000', '360502000000', '渝水区');
INSERT INTO `j_position_county` VALUES ('1142', '360500000000', '360521000000', '分宜县');
INSERT INTO `j_position_county` VALUES ('1143', '360600000000', '360602000000', '月湖区');
INSERT INTO `j_position_county` VALUES ('1144', '360600000000', '360622000000', '余江县');
INSERT INTO `j_position_county` VALUES ('1145', '360600000000', '360681000000', '贵溪市');
INSERT INTO `j_position_county` VALUES ('1146', '360700000000', '360702000000', '章贡区');
INSERT INTO `j_position_county` VALUES ('1147', '360700000000', '360721000000', '赣县');
INSERT INTO `j_position_county` VALUES ('1148', '360700000000', '360722000000', '信丰县');
INSERT INTO `j_position_county` VALUES ('1149', '360700000000', '360723000000', '大余县');
INSERT INTO `j_position_county` VALUES ('1150', '360700000000', '360724000000', '上犹县');
INSERT INTO `j_position_county` VALUES ('1151', '360700000000', '360725000000', '崇义县');
INSERT INTO `j_position_county` VALUES ('1152', '360700000000', '360726000000', '安远县');
INSERT INTO `j_position_county` VALUES ('1153', '360700000000', '360727000000', '龙南县');
INSERT INTO `j_position_county` VALUES ('1154', '360700000000', '360728000000', '定南县');
INSERT INTO `j_position_county` VALUES ('1155', '360700000000', '360729000000', '全南县');
INSERT INTO `j_position_county` VALUES ('1156', '360700000000', '360730000000', '宁都县');
INSERT INTO `j_position_county` VALUES ('1157', '360700000000', '360731000000', '于都县');
INSERT INTO `j_position_county` VALUES ('1158', '360700000000', '360732000000', '兴国县');
INSERT INTO `j_position_county` VALUES ('1159', '360700000000', '360733000000', '会昌县');
INSERT INTO `j_position_county` VALUES ('1160', '360700000000', '360734000000', '寻乌县');
INSERT INTO `j_position_county` VALUES ('1161', '360700000000', '360735000000', '石城县');
INSERT INTO `j_position_county` VALUES ('1162', '360700000000', '360781000000', '瑞金市');
INSERT INTO `j_position_county` VALUES ('1163', '360700000000', '360782000000', '南康市');
INSERT INTO `j_position_county` VALUES ('1164', '360800000000', '360802000000', '吉州区');
INSERT INTO `j_position_county` VALUES ('1165', '360800000000', '360803000000', '青原区');
INSERT INTO `j_position_county` VALUES ('1166', '360800000000', '360821000000', '吉安县');
INSERT INTO `j_position_county` VALUES ('1167', '360800000000', '360822000000', '吉水县');
INSERT INTO `j_position_county` VALUES ('1168', '360800000000', '360823000000', '峡江县');
INSERT INTO `j_position_county` VALUES ('1169', '360800000000', '360824000000', '新干县');
INSERT INTO `j_position_county` VALUES ('1170', '360800000000', '360825000000', '永丰县');
INSERT INTO `j_position_county` VALUES ('1171', '360800000000', '360826000000', '泰和县');
INSERT INTO `j_position_county` VALUES ('1172', '360800000000', '360827000000', '遂川县');
INSERT INTO `j_position_county` VALUES ('1173', '360800000000', '360828000000', '万安县');
INSERT INTO `j_position_county` VALUES ('1174', '360800000000', '360829000000', '安福县');
INSERT INTO `j_position_county` VALUES ('1175', '360800000000', '360830000000', '永新县');
INSERT INTO `j_position_county` VALUES ('1176', '360800000000', '360881000000', '井冈山市');
INSERT INTO `j_position_county` VALUES ('1177', '360900000000', '360902000000', '袁州区');
INSERT INTO `j_position_county` VALUES ('1178', '360900000000', '360921000000', '奉新县');
INSERT INTO `j_position_county` VALUES ('1179', '360900000000', '360922000000', '万载县');
INSERT INTO `j_position_county` VALUES ('1180', '360900000000', '360923000000', '上高县');
INSERT INTO `j_position_county` VALUES ('1181', '360900000000', '360924000000', '宜丰县');
INSERT INTO `j_position_county` VALUES ('1182', '360900000000', '360925000000', '靖安县');
INSERT INTO `j_position_county` VALUES ('1183', '360900000000', '360926000000', '铜鼓县');
INSERT INTO `j_position_county` VALUES ('1184', '360900000000', '360981000000', '丰城市');
INSERT INTO `j_position_county` VALUES ('1185', '360900000000', '360982000000', '樟树市');
INSERT INTO `j_position_county` VALUES ('1186', '360900000000', '360983000000', '高安市');
INSERT INTO `j_position_county` VALUES ('1187', '361000000000', '361002000000', '临川区');
INSERT INTO `j_position_county` VALUES ('1188', '361000000000', '361021000000', '南城县');
INSERT INTO `j_position_county` VALUES ('1189', '361000000000', '361022000000', '黎川县');
INSERT INTO `j_position_county` VALUES ('1190', '361000000000', '361023000000', '南丰县');
INSERT INTO `j_position_county` VALUES ('1191', '361000000000', '361024000000', '崇仁县');
INSERT INTO `j_position_county` VALUES ('1192', '361000000000', '361025000000', '乐安县');
INSERT INTO `j_position_county` VALUES ('1193', '361000000000', '361026000000', '宜黄县');
INSERT INTO `j_position_county` VALUES ('1194', '361000000000', '361027000000', '金溪县');
INSERT INTO `j_position_county` VALUES ('1195', '361000000000', '361028000000', '资溪县');
INSERT INTO `j_position_county` VALUES ('1196', '361000000000', '361029000000', '东乡县');
INSERT INTO `j_position_county` VALUES ('1197', '361000000000', '361030000000', '广昌县');
INSERT INTO `j_position_county` VALUES ('1198', '361100000000', '361102000000', '信州区');
INSERT INTO `j_position_county` VALUES ('1199', '361100000000', '361121000000', '上饶县');
INSERT INTO `j_position_county` VALUES ('1200', '361100000000', '361122000000', '广丰县');
INSERT INTO `j_position_county` VALUES ('1201', '361100000000', '361123000000', '玉山县');
INSERT INTO `j_position_county` VALUES ('1202', '361100000000', '361124000000', '铅山县');
INSERT INTO `j_position_county` VALUES ('1203', '361100000000', '361125000000', '横峰县');
INSERT INTO `j_position_county` VALUES ('1204', '361100000000', '361126000000', '弋阳县');
INSERT INTO `j_position_county` VALUES ('1205', '361100000000', '361127000000', '余干县');
INSERT INTO `j_position_county` VALUES ('1206', '361100000000', '361128000000', '鄱阳县');
INSERT INTO `j_position_county` VALUES ('1207', '361100000000', '361129000000', '万年县');
INSERT INTO `j_position_county` VALUES ('1208', '361100000000', '361130000000', '婺源县');
INSERT INTO `j_position_county` VALUES ('1209', '361100000000', '361181000000', '德兴市');
INSERT INTO `j_position_county` VALUES ('1210', '370100000000', '370102000000', '历下区');
INSERT INTO `j_position_county` VALUES ('1211', '370100000000', '370103000000', '市中区');
INSERT INTO `j_position_county` VALUES ('1212', '370100000000', '370104000000', '槐荫区');
INSERT INTO `j_position_county` VALUES ('1213', '370100000000', '370105000000', '天桥区');
INSERT INTO `j_position_county` VALUES ('1214', '370100000000', '370112000000', '历城区');
INSERT INTO `j_position_county` VALUES ('1215', '370100000000', '370113000000', '长清区');
INSERT INTO `j_position_county` VALUES ('1216', '370100000000', '370124000000', '平阴县');
INSERT INTO `j_position_county` VALUES ('1217', '370100000000', '370125000000', '济阳县');
INSERT INTO `j_position_county` VALUES ('1218', '370100000000', '370126000000', '商河县');
INSERT INTO `j_position_county` VALUES ('1219', '370100000000', '370181000000', '章丘市');
INSERT INTO `j_position_county` VALUES ('1220', '370200000000', '370202000000', '市南区');
INSERT INTO `j_position_county` VALUES ('1221', '370200000000', '370203000000', '市北区');
INSERT INTO `j_position_county` VALUES ('1222', '370200000000', '370211000000', '黄岛区');
INSERT INTO `j_position_county` VALUES ('1223', '370200000000', '370212000000', '崂山区');
INSERT INTO `j_position_county` VALUES ('1224', '370200000000', '370213000000', '李沧区');
INSERT INTO `j_position_county` VALUES ('1225', '370200000000', '370214000000', '城阳区');
INSERT INTO `j_position_county` VALUES ('1226', '370200000000', '370281000000', '胶州市');
INSERT INTO `j_position_county` VALUES ('1227', '370200000000', '370282000000', '即墨市');
INSERT INTO `j_position_county` VALUES ('1228', '370200000000', '370283000000', '平度市');
INSERT INTO `j_position_county` VALUES ('1229', '370200000000', '370285000000', '莱西市');
INSERT INTO `j_position_county` VALUES ('1230', '370300000000', '370302000000', '淄川区');
INSERT INTO `j_position_county` VALUES ('1231', '370300000000', '370303000000', '张店区');
INSERT INTO `j_position_county` VALUES ('1232', '370300000000', '370304000000', '博山区');
INSERT INTO `j_position_county` VALUES ('1233', '370300000000', '370305000000', '临淄区');
INSERT INTO `j_position_county` VALUES ('1234', '370300000000', '370306000000', '周村区');
INSERT INTO `j_position_county` VALUES ('1235', '370300000000', '370321000000', '桓台县');
INSERT INTO `j_position_county` VALUES ('1236', '370300000000', '370322000000', '高青县');
INSERT INTO `j_position_county` VALUES ('1237', '370300000000', '370323000000', '沂源县');
INSERT INTO `j_position_county` VALUES ('1238', '370400000000', '370402000000', '市中区');
INSERT INTO `j_position_county` VALUES ('1239', '370400000000', '370403000000', '薛城区');
INSERT INTO `j_position_county` VALUES ('1240', '370400000000', '370404000000', '峄城区');
INSERT INTO `j_position_county` VALUES ('1241', '370400000000', '370405000000', '台儿庄区');
INSERT INTO `j_position_county` VALUES ('1242', '370400000000', '370406000000', '山亭区');
INSERT INTO `j_position_county` VALUES ('1243', '370400000000', '370481000000', '滕州市');
INSERT INTO `j_position_county` VALUES ('1244', '370500000000', '370502000000', '东营区');
INSERT INTO `j_position_county` VALUES ('1245', '370500000000', '370503000000', '河口区');
INSERT INTO `j_position_county` VALUES ('1246', '370500000000', '370521000000', '垦利县');
INSERT INTO `j_position_county` VALUES ('1247', '370500000000', '370522000000', '利津县');
INSERT INTO `j_position_county` VALUES ('1248', '370500000000', '370523000000', '广饶县');
INSERT INTO `j_position_county` VALUES ('1249', '370600000000', '370602000000', '芝罘区');
INSERT INTO `j_position_county` VALUES ('1250', '370600000000', '370611000000', '福山区');
INSERT INTO `j_position_county` VALUES ('1251', '370600000000', '370612000000', '牟平区');
INSERT INTO `j_position_county` VALUES ('1252', '370600000000', '370613000000', '莱山区');
INSERT INTO `j_position_county` VALUES ('1253', '370600000000', '370634000000', '长岛县');
INSERT INTO `j_position_county` VALUES ('1254', '370600000000', '370681000000', '龙口市');
INSERT INTO `j_position_county` VALUES ('1255', '370600000000', '370682000000', '莱阳市');
INSERT INTO `j_position_county` VALUES ('1256', '370600000000', '370683000000', '莱州市');
INSERT INTO `j_position_county` VALUES ('1257', '370600000000', '370684000000', '蓬莱市');
INSERT INTO `j_position_county` VALUES ('1258', '370600000000', '370685000000', '招远市');
INSERT INTO `j_position_county` VALUES ('1259', '370600000000', '370686000000', '栖霞市');
INSERT INTO `j_position_county` VALUES ('1260', '370600000000', '370687000000', '海阳市');
INSERT INTO `j_position_county` VALUES ('1261', '370700000000', '370702000000', '潍城区');
INSERT INTO `j_position_county` VALUES ('1262', '370700000000', '370703000000', '寒亭区');
INSERT INTO `j_position_county` VALUES ('1263', '370700000000', '370704000000', '坊子区');
INSERT INTO `j_position_county` VALUES ('1264', '370700000000', '370705000000', '奎文区');
INSERT INTO `j_position_county` VALUES ('1265', '370700000000', '370724000000', '临朐县');
INSERT INTO `j_position_county` VALUES ('1266', '370700000000', '370725000000', '昌乐县');
INSERT INTO `j_position_county` VALUES ('1267', '370700000000', '370781000000', '青州市');
INSERT INTO `j_position_county` VALUES ('1268', '370700000000', '370782000000', '诸城市');
INSERT INTO `j_position_county` VALUES ('1269', '370700000000', '370783000000', '寿光市');
INSERT INTO `j_position_county` VALUES ('1270', '370700000000', '370784000000', '安丘市');
INSERT INTO `j_position_county` VALUES ('1271', '370700000000', '370785000000', '高密市');
INSERT INTO `j_position_county` VALUES ('1272', '370700000000', '370786000000', '昌邑市');
INSERT INTO `j_position_county` VALUES ('1273', '370800000000', '370802000000', '市中区');
INSERT INTO `j_position_county` VALUES ('1274', '370800000000', '370811000000', '任城区');
INSERT INTO `j_position_county` VALUES ('1275', '370800000000', '370826000000', '微山县');
INSERT INTO `j_position_county` VALUES ('1276', '370800000000', '370827000000', '鱼台县');
INSERT INTO `j_position_county` VALUES ('1277', '370800000000', '370828000000', '金乡县');
INSERT INTO `j_position_county` VALUES ('1278', '370800000000', '370829000000', '嘉祥县');
INSERT INTO `j_position_county` VALUES ('1279', '370800000000', '370830000000', '汶上县');
INSERT INTO `j_position_county` VALUES ('1280', '370800000000', '370831000000', '泗水县');
INSERT INTO `j_position_county` VALUES ('1281', '370800000000', '370832000000', '梁山县');
INSERT INTO `j_position_county` VALUES ('1282', '370800000000', '370881000000', '曲阜市');
INSERT INTO `j_position_county` VALUES ('1283', '370800000000', '370882000000', '兖州市');
INSERT INTO `j_position_county` VALUES ('1284', '370800000000', '370883000000', '邹城市');
INSERT INTO `j_position_county` VALUES ('1285', '370900000000', '370902000000', '泰山区');
INSERT INTO `j_position_county` VALUES ('1286', '370900000000', '370911000000', '岱岳区');
INSERT INTO `j_position_county` VALUES ('1287', '370900000000', '370921000000', '宁阳县');
INSERT INTO `j_position_county` VALUES ('1288', '370900000000', '370923000000', '东平县');
INSERT INTO `j_position_county` VALUES ('1289', '370900000000', '370982000000', '新泰市');
INSERT INTO `j_position_county` VALUES ('1290', '370900000000', '370983000000', '肥城市');
INSERT INTO `j_position_county` VALUES ('1291', '371000000000', '371002000000', '环翠区');
INSERT INTO `j_position_county` VALUES ('1292', '371000000000', '371081000000', '文登市');
INSERT INTO `j_position_county` VALUES ('1293', '371000000000', '371082000000', '荣成市');
INSERT INTO `j_position_county` VALUES ('1294', '371000000000', '371083000000', '乳山市');
INSERT INTO `j_position_county` VALUES ('1295', '371100000000', '371102000000', '东港区');
INSERT INTO `j_position_county` VALUES ('1296', '371100000000', '371103000000', '岚山区');
INSERT INTO `j_position_county` VALUES ('1297', '371100000000', '371121000000', '五莲县');
INSERT INTO `j_position_county` VALUES ('1298', '371100000000', '371122000000', '莒县');
INSERT INTO `j_position_county` VALUES ('1299', '371200000000', '371202000000', '莱城区');
INSERT INTO `j_position_county` VALUES ('1300', '371200000000', '371203000000', '钢城区');
INSERT INTO `j_position_county` VALUES ('1301', '371300000000', '371302000000', '兰山区');
INSERT INTO `j_position_county` VALUES ('1302', '371300000000', '371311000000', '罗庄区');
INSERT INTO `j_position_county` VALUES ('1303', '371300000000', '371312000000', '河东区');
INSERT INTO `j_position_county` VALUES ('1304', '371300000000', '371321000000', '沂南县');
INSERT INTO `j_position_county` VALUES ('1305', '371300000000', '371322000000', '郯城县');
INSERT INTO `j_position_county` VALUES ('1306', '371300000000', '371323000000', '沂水县');
INSERT INTO `j_position_county` VALUES ('1307', '371300000000', '371324000000', '苍山县');
INSERT INTO `j_position_county` VALUES ('1308', '371300000000', '371325000000', '费县');
INSERT INTO `j_position_county` VALUES ('1309', '371300000000', '371326000000', '平邑县');
INSERT INTO `j_position_county` VALUES ('1310', '371300000000', '371327000000', '莒南县');
INSERT INTO `j_position_county` VALUES ('1311', '371300000000', '371328000000', '蒙阴县');
INSERT INTO `j_position_county` VALUES ('1312', '371300000000', '371329000000', '临沭县');
INSERT INTO `j_position_county` VALUES ('1313', '371400000000', '371402000000', '德城区');
INSERT INTO `j_position_county` VALUES ('1314', '371400000000', '371421000000', '陵县');
INSERT INTO `j_position_county` VALUES ('1315', '371400000000', '371422000000', '宁津县');
INSERT INTO `j_position_county` VALUES ('1316', '371400000000', '371423000000', '庆云县');
INSERT INTO `j_position_county` VALUES ('1317', '371400000000', '371424000000', '临邑县');
INSERT INTO `j_position_county` VALUES ('1318', '371400000000', '371425000000', '齐河县');
INSERT INTO `j_position_county` VALUES ('1319', '371400000000', '371426000000', '平原县');
INSERT INTO `j_position_county` VALUES ('1320', '371400000000', '371427000000', '夏津县');
INSERT INTO `j_position_county` VALUES ('1321', '371400000000', '371428000000', '武城县');
INSERT INTO `j_position_county` VALUES ('1322', '371400000000', '371481000000', '乐陵市');
INSERT INTO `j_position_county` VALUES ('1323', '371400000000', '371482000000', '禹城市');
INSERT INTO `j_position_county` VALUES ('1324', '371500000000', '371502000000', '东昌府区');
INSERT INTO `j_position_county` VALUES ('1325', '371500000000', '371521000000', '阳谷县');
INSERT INTO `j_position_county` VALUES ('1326', '371500000000', '371522000000', '莘县');
INSERT INTO `j_position_county` VALUES ('1327', '371500000000', '371523000000', '茌平县');
INSERT INTO `j_position_county` VALUES ('1328', '371500000000', '371524000000', '东阿县');
INSERT INTO `j_position_county` VALUES ('1329', '371500000000', '371525000000', '冠县');
INSERT INTO `j_position_county` VALUES ('1330', '371500000000', '371526000000', '高唐县');
INSERT INTO `j_position_county` VALUES ('1331', '371500000000', '371581000000', '临清市');
INSERT INTO `j_position_county` VALUES ('1332', '371600000000', '371602000000', '滨城区');
INSERT INTO `j_position_county` VALUES ('1333', '371600000000', '371621000000', '惠民县');
INSERT INTO `j_position_county` VALUES ('1334', '371600000000', '371622000000', '阳信县');
INSERT INTO `j_position_county` VALUES ('1335', '371600000000', '371623000000', '无棣县');
INSERT INTO `j_position_county` VALUES ('1336', '371600000000', '371624000000', '沾化县');
INSERT INTO `j_position_county` VALUES ('1337', '371600000000', '371625000000', '博兴县');
INSERT INTO `j_position_county` VALUES ('1338', '371600000000', '371626000000', '邹平县');
INSERT INTO `j_position_county` VALUES ('1339', '371700000000', '371702000000', '牡丹区');
INSERT INTO `j_position_county` VALUES ('1340', '371700000000', '371721000000', '曹县');
INSERT INTO `j_position_county` VALUES ('1341', '371700000000', '371722000000', '单县');
INSERT INTO `j_position_county` VALUES ('1342', '371700000000', '371723000000', '成武县');
INSERT INTO `j_position_county` VALUES ('1343', '371700000000', '371724000000', '巨野县');
INSERT INTO `j_position_county` VALUES ('1344', '371700000000', '371725000000', '郓城县');
INSERT INTO `j_position_county` VALUES ('1345', '371700000000', '371726000000', '鄄城县');
INSERT INTO `j_position_county` VALUES ('1346', '371700000000', '371727000000', '定陶县');
INSERT INTO `j_position_county` VALUES ('1347', '371700000000', '371728000000', '东明县');
INSERT INTO `j_position_county` VALUES ('1348', '410100000000', '410102000000', '中原区');
INSERT INTO `j_position_county` VALUES ('1349', '410100000000', '410103000000', '二七区');
INSERT INTO `j_position_county` VALUES ('1350', '410100000000', '410104000000', '管城回族区');
INSERT INTO `j_position_county` VALUES ('1351', '410100000000', '410105000000', '金水区');
INSERT INTO `j_position_county` VALUES ('1352', '410100000000', '410106000000', '上街区');
INSERT INTO `j_position_county` VALUES ('1353', '410100000000', '410108000000', '惠济区');
INSERT INTO `j_position_county` VALUES ('1354', '410100000000', '410122000000', '中牟县');
INSERT INTO `j_position_county` VALUES ('1355', '410100000000', '410181000000', '巩义市');
INSERT INTO `j_position_county` VALUES ('1356', '410100000000', '410182000000', '荥阳市');
INSERT INTO `j_position_county` VALUES ('1357', '410100000000', '410183000000', '新密市');
INSERT INTO `j_position_county` VALUES ('1358', '410100000000', '410184000000', '新郑市');
INSERT INTO `j_position_county` VALUES ('1359', '410100000000', '410185000000', '登封市');
INSERT INTO `j_position_county` VALUES ('1360', '410200000000', '410202000000', '龙亭区');
INSERT INTO `j_position_county` VALUES ('1361', '410200000000', '410203000000', '顺河回族区');
INSERT INTO `j_position_county` VALUES ('1362', '410200000000', '410204000000', '鼓楼区');
INSERT INTO `j_position_county` VALUES ('1363', '410200000000', '410205000000', '禹王台区');
INSERT INTO `j_position_county` VALUES ('1364', '410200000000', '410211000000', '金明区');
INSERT INTO `j_position_county` VALUES ('1365', '410200000000', '410221000000', '杞县');
INSERT INTO `j_position_county` VALUES ('1366', '410200000000', '410222000000', '通许县');
INSERT INTO `j_position_county` VALUES ('1367', '410200000000', '410223000000', '尉氏县');
INSERT INTO `j_position_county` VALUES ('1368', '410200000000', '410224000000', '开封县');
INSERT INTO `j_position_county` VALUES ('1369', '410200000000', '410225000000', '兰考县');
INSERT INTO `j_position_county` VALUES ('1370', '410300000000', '410302000000', '老城区');
INSERT INTO `j_position_county` VALUES ('1371', '410300000000', '410303000000', '西工区');
INSERT INTO `j_position_county` VALUES ('1372', '410300000000', '410304000000', '瀍河回族区');
INSERT INTO `j_position_county` VALUES ('1373', '410300000000', '410305000000', '涧西区');
INSERT INTO `j_position_county` VALUES ('1374', '410300000000', '410306000000', '吉利区');
INSERT INTO `j_position_county` VALUES ('1375', '410300000000', '410311000000', '洛龙区');
INSERT INTO `j_position_county` VALUES ('1376', '410300000000', '410322000000', '孟津县');
INSERT INTO `j_position_county` VALUES ('1377', '410300000000', '410323000000', '新安县');
INSERT INTO `j_position_county` VALUES ('1378', '410300000000', '410324000000', '栾川县');
INSERT INTO `j_position_county` VALUES ('1379', '410300000000', '410325000000', '嵩县');
INSERT INTO `j_position_county` VALUES ('1380', '410300000000', '410326000000', '汝阳县');
INSERT INTO `j_position_county` VALUES ('1381', '410300000000', '410327000000', '宜阳县');
INSERT INTO `j_position_county` VALUES ('1382', '410300000000', '410328000000', '洛宁县');
INSERT INTO `j_position_county` VALUES ('1383', '410300000000', '410329000000', '伊川县');
INSERT INTO `j_position_county` VALUES ('1384', '410300000000', '410381000000', '偃师市');
INSERT INTO `j_position_county` VALUES ('1385', '410400000000', '410402000000', '新华区');
INSERT INTO `j_position_county` VALUES ('1386', '410400000000', '410403000000', '卫东区');
INSERT INTO `j_position_county` VALUES ('1387', '410400000000', '410404000000', '石龙区');
INSERT INTO `j_position_county` VALUES ('1388', '410400000000', '410411000000', '湛河区');
INSERT INTO `j_position_county` VALUES ('1389', '410400000000', '410421000000', '宝丰县');
INSERT INTO `j_position_county` VALUES ('1390', '410400000000', '410422000000', '叶县');
INSERT INTO `j_position_county` VALUES ('1391', '410400000000', '410423000000', '鲁山县');
INSERT INTO `j_position_county` VALUES ('1392', '410400000000', '410425000000', '郏县');
INSERT INTO `j_position_county` VALUES ('1393', '410400000000', '410481000000', '舞钢市');
INSERT INTO `j_position_county` VALUES ('1394', '410400000000', '410482000000', '汝州市');
INSERT INTO `j_position_county` VALUES ('1395', '410500000000', '410502000000', '文峰区');
INSERT INTO `j_position_county` VALUES ('1396', '410500000000', '410503000000', '北关区');
INSERT INTO `j_position_county` VALUES ('1397', '410500000000', '410505000000', '殷都区');
INSERT INTO `j_position_county` VALUES ('1398', '410500000000', '410506000000', '龙安区');
INSERT INTO `j_position_county` VALUES ('1399', '410500000000', '410522000000', '安阳县');
INSERT INTO `j_position_county` VALUES ('1400', '410500000000', '410523000000', '汤阴县');
INSERT INTO `j_position_county` VALUES ('1401', '410500000000', '410526000000', '滑县');
INSERT INTO `j_position_county` VALUES ('1402', '410500000000', '410527000000', '内黄县');
INSERT INTO `j_position_county` VALUES ('1403', '410500000000', '410581000000', '林州市');
INSERT INTO `j_position_county` VALUES ('1404', '410600000000', '410602000000', '鹤山区');
INSERT INTO `j_position_county` VALUES ('1405', '410600000000', '410603000000', '山城区');
INSERT INTO `j_position_county` VALUES ('1406', '410600000000', '410611000000', '淇滨区');
INSERT INTO `j_position_county` VALUES ('1407', '410600000000', '410621000000', '浚县');
INSERT INTO `j_position_county` VALUES ('1408', '410600000000', '410622000000', '淇县');
INSERT INTO `j_position_county` VALUES ('1409', '410700000000', '410702000000', '红旗区');
INSERT INTO `j_position_county` VALUES ('1410', '410700000000', '410703000000', '卫滨区');
INSERT INTO `j_position_county` VALUES ('1411', '410700000000', '410704000000', '凤泉区');
INSERT INTO `j_position_county` VALUES ('1412', '410700000000', '410711000000', '牧野区');
INSERT INTO `j_position_county` VALUES ('1413', '410700000000', '410721000000', '新乡县');
INSERT INTO `j_position_county` VALUES ('1414', '410700000000', '410724000000', '获嘉县');
INSERT INTO `j_position_county` VALUES ('1415', '410700000000', '410725000000', '原阳县');
INSERT INTO `j_position_county` VALUES ('1416', '410700000000', '410726000000', '延津县');
INSERT INTO `j_position_county` VALUES ('1417', '410700000000', '410727000000', '封丘县');
INSERT INTO `j_position_county` VALUES ('1418', '410700000000', '410728000000', '长垣县');
INSERT INTO `j_position_county` VALUES ('1419', '410700000000', '410781000000', '卫辉市');
INSERT INTO `j_position_county` VALUES ('1420', '410700000000', '410782000000', '辉县市');
INSERT INTO `j_position_county` VALUES ('1421', '410800000000', '410802000000', '解放区');
INSERT INTO `j_position_county` VALUES ('1422', '410800000000', '410803000000', '中站区');
INSERT INTO `j_position_county` VALUES ('1423', '410800000000', '410804000000', '马村区');
INSERT INTO `j_position_county` VALUES ('1424', '410800000000', '410811000000', '山阳区');
INSERT INTO `j_position_county` VALUES ('1425', '410800000000', '410821000000', '修武县');
INSERT INTO `j_position_county` VALUES ('1426', '410800000000', '410822000000', '博爱县');
INSERT INTO `j_position_county` VALUES ('1427', '410800000000', '410823000000', '武陟县');
INSERT INTO `j_position_county` VALUES ('1428', '410800000000', '410825000000', '温县');
INSERT INTO `j_position_county` VALUES ('1429', '410800000000', '410882000000', '沁阳市');
INSERT INTO `j_position_county` VALUES ('1430', '410800000000', '410883000000', '孟州市');
INSERT INTO `j_position_county` VALUES ('1431', '410900000000', '410902000000', '华龙区');
INSERT INTO `j_position_county` VALUES ('1432', '410900000000', '410922000000', '清丰县');
INSERT INTO `j_position_county` VALUES ('1433', '410900000000', '410923000000', '南乐县');
INSERT INTO `j_position_county` VALUES ('1434', '410900000000', '410926000000', '范县');
INSERT INTO `j_position_county` VALUES ('1435', '410900000000', '410927000000', '台前县');
INSERT INTO `j_position_county` VALUES ('1436', '410900000000', '410928000000', '濮阳县');
INSERT INTO `j_position_county` VALUES ('1437', '411000000000', '411002000000', '魏都区');
INSERT INTO `j_position_county` VALUES ('1438', '411000000000', '411023000000', '许昌县');
INSERT INTO `j_position_county` VALUES ('1439', '411000000000', '411024000000', '鄢陵县');
INSERT INTO `j_position_county` VALUES ('1440', '411000000000', '411025000000', '襄城县');
INSERT INTO `j_position_county` VALUES ('1441', '411000000000', '411081000000', '禹州市');
INSERT INTO `j_position_county` VALUES ('1442', '411000000000', '411082000000', '长葛市');
INSERT INTO `j_position_county` VALUES ('1443', '411100000000', '411102000000', '源汇区');
INSERT INTO `j_position_county` VALUES ('1444', '411100000000', '411103000000', '郾城区');
INSERT INTO `j_position_county` VALUES ('1445', '411100000000', '411104000000', '召陵区');
INSERT INTO `j_position_county` VALUES ('1446', '411100000000', '411121000000', '舞阳县');
INSERT INTO `j_position_county` VALUES ('1447', '411100000000', '411122000000', '临颍县');
INSERT INTO `j_position_county` VALUES ('1448', '411200000000', '411202000000', '湖滨区');
INSERT INTO `j_position_county` VALUES ('1449', '411200000000', '411221000000', '渑池县');
INSERT INTO `j_position_county` VALUES ('1450', '411200000000', '411222000000', '陕县');
INSERT INTO `j_position_county` VALUES ('1451', '411200000000', '411224000000', '卢氏县');
INSERT INTO `j_position_county` VALUES ('1452', '411200000000', '411281000000', '义马市');
INSERT INTO `j_position_county` VALUES ('1453', '411200000000', '411282000000', '灵宝市');
INSERT INTO `j_position_county` VALUES ('1454', '411300000000', '411302000000', '宛城区');
INSERT INTO `j_position_county` VALUES ('1455', '411300000000', '411303000000', '卧龙区');
INSERT INTO `j_position_county` VALUES ('1456', '411300000000', '411321000000', '南召县');
INSERT INTO `j_position_county` VALUES ('1457', '411300000000', '411322000000', '方城县');
INSERT INTO `j_position_county` VALUES ('1458', '411300000000', '411323000000', '西峡县');
INSERT INTO `j_position_county` VALUES ('1459', '411300000000', '411324000000', '镇平县');
INSERT INTO `j_position_county` VALUES ('1460', '411300000000', '411325000000', '内乡县');
INSERT INTO `j_position_county` VALUES ('1461', '411300000000', '411326000000', '淅川县');
INSERT INTO `j_position_county` VALUES ('1462', '411300000000', '411327000000', '社旗县');
INSERT INTO `j_position_county` VALUES ('1463', '411300000000', '411328000000', '唐河县');
INSERT INTO `j_position_county` VALUES ('1464', '411300000000', '411329000000', '新野县');
INSERT INTO `j_position_county` VALUES ('1465', '411300000000', '411330000000', '桐柏县');
INSERT INTO `j_position_county` VALUES ('1466', '411300000000', '411381000000', '邓州市');
INSERT INTO `j_position_county` VALUES ('1467', '411400000000', '411402000000', '梁园区');
INSERT INTO `j_position_county` VALUES ('1468', '411400000000', '411403000000', '睢阳区');
INSERT INTO `j_position_county` VALUES ('1469', '411400000000', '411421000000', '民权县');
INSERT INTO `j_position_county` VALUES ('1470', '411400000000', '411422000000', '睢县');
INSERT INTO `j_position_county` VALUES ('1471', '411400000000', '411423000000', '宁陵县');
INSERT INTO `j_position_county` VALUES ('1472', '411400000000', '411424000000', '柘城县');
INSERT INTO `j_position_county` VALUES ('1473', '411400000000', '411425000000', '虞城县');
INSERT INTO `j_position_county` VALUES ('1474', '411400000000', '411426000000', '夏邑县');
INSERT INTO `j_position_county` VALUES ('1475', '411400000000', '411481000000', '永城市');
INSERT INTO `j_position_county` VALUES ('1476', '411500000000', '411502000000', '浉河区');
INSERT INTO `j_position_county` VALUES ('1477', '411500000000', '411503000000', '平桥区');
INSERT INTO `j_position_county` VALUES ('1478', '411500000000', '411521000000', '罗山县');
INSERT INTO `j_position_county` VALUES ('1479', '411500000000', '411522000000', '光山县');
INSERT INTO `j_position_county` VALUES ('1480', '411500000000', '411523000000', '新县');
INSERT INTO `j_position_county` VALUES ('1481', '411500000000', '411524000000', '商城县');
INSERT INTO `j_position_county` VALUES ('1482', '411500000000', '411525000000', '固始县');
INSERT INTO `j_position_county` VALUES ('1483', '411500000000', '411526000000', '潢川县');
INSERT INTO `j_position_county` VALUES ('1484', '411500000000', '411527000000', '淮滨县');
INSERT INTO `j_position_county` VALUES ('1485', '411500000000', '411528000000', '息县');
INSERT INTO `j_position_county` VALUES ('1486', '411600000000', '411602000000', '川汇区');
INSERT INTO `j_position_county` VALUES ('1487', '411600000000', '411621000000', '扶沟县');
INSERT INTO `j_position_county` VALUES ('1488', '411600000000', '411622000000', '西华县');
INSERT INTO `j_position_county` VALUES ('1489', '411600000000', '411623000000', '商水县');
INSERT INTO `j_position_county` VALUES ('1490', '411600000000', '411624000000', '沈丘县');
INSERT INTO `j_position_county` VALUES ('1491', '411600000000', '411625000000', '郸城县');
INSERT INTO `j_position_county` VALUES ('1492', '411600000000', '411626000000', '淮阳县');
INSERT INTO `j_position_county` VALUES ('1493', '411600000000', '411627000000', '太康县');
INSERT INTO `j_position_county` VALUES ('1494', '411600000000', '411628000000', '鹿邑县');
INSERT INTO `j_position_county` VALUES ('1495', '411600000000', '411681000000', '项城市');
INSERT INTO `j_position_county` VALUES ('1496', '411700000000', '411702000000', '驿城区');
INSERT INTO `j_position_county` VALUES ('1497', '411700000000', '411721000000', '西平县');
INSERT INTO `j_position_county` VALUES ('1498', '411700000000', '411722000000', '上蔡县');
INSERT INTO `j_position_county` VALUES ('1499', '411700000000', '411723000000', '平舆县');
INSERT INTO `j_position_county` VALUES ('1500', '411700000000', '411724000000', '正阳县');
INSERT INTO `j_position_county` VALUES ('1501', '411700000000', '411725000000', '确山县');
INSERT INTO `j_position_county` VALUES ('1502', '411700000000', '411726000000', '泌阳县');
INSERT INTO `j_position_county` VALUES ('1503', '411700000000', '411727000000', '汝南县');
INSERT INTO `j_position_county` VALUES ('1504', '411700000000', '411728000000', '遂平县');
INSERT INTO `j_position_county` VALUES ('1505', '411700000000', '411729000000', '新蔡县');
INSERT INTO `j_position_county` VALUES ('1506', '419000000000', '419001000000', '济源市');
INSERT INTO `j_position_county` VALUES ('1507', '420100000000', '420102000000', '江岸区');
INSERT INTO `j_position_county` VALUES ('1508', '420100000000', '420103000000', '江汉区');
INSERT INTO `j_position_county` VALUES ('1509', '420100000000', '420104000000', '硚口区');
INSERT INTO `j_position_county` VALUES ('1510', '420100000000', '420105000000', '汉阳区');
INSERT INTO `j_position_county` VALUES ('1511', '420100000000', '420106000000', '武昌区');
INSERT INTO `j_position_county` VALUES ('1512', '420100000000', '420107000000', '青山区');
INSERT INTO `j_position_county` VALUES ('1513', '420100000000', '420111000000', '洪山区');
INSERT INTO `j_position_county` VALUES ('1514', '420100000000', '420112000000', '东西湖区');
INSERT INTO `j_position_county` VALUES ('1515', '420100000000', '420113000000', '汉南区');
INSERT INTO `j_position_county` VALUES ('1516', '420100000000', '420114000000', '蔡甸区');
INSERT INTO `j_position_county` VALUES ('1517', '420100000000', '420115000000', '江夏区');
INSERT INTO `j_position_county` VALUES ('1518', '420100000000', '420116000000', '黄陂区');
INSERT INTO `j_position_county` VALUES ('1519', '420100000000', '420117000000', '新洲区');
INSERT INTO `j_position_county` VALUES ('1520', '420200000000', '420202000000', '黄石港区');
INSERT INTO `j_position_county` VALUES ('1521', '420200000000', '420203000000', '西塞山区');
INSERT INTO `j_position_county` VALUES ('1522', '420200000000', '420204000000', '下陆区');
INSERT INTO `j_position_county` VALUES ('1523', '420200000000', '420205000000', '铁山区');
INSERT INTO `j_position_county` VALUES ('1524', '420200000000', '420222000000', '阳新县');
INSERT INTO `j_position_county` VALUES ('1525', '420200000000', '420281000000', '大冶市');
INSERT INTO `j_position_county` VALUES ('1526', '420300000000', '420302000000', '茅箭区');
INSERT INTO `j_position_county` VALUES ('1527', '420300000000', '420303000000', '张湾区');
INSERT INTO `j_position_county` VALUES ('1528', '420300000000', '420321000000', '郧县');
INSERT INTO `j_position_county` VALUES ('1529', '420300000000', '420322000000', '郧西县');
INSERT INTO `j_position_county` VALUES ('1530', '420300000000', '420323000000', '竹山县');
INSERT INTO `j_position_county` VALUES ('1531', '420300000000', '420324000000', '竹溪县');
INSERT INTO `j_position_county` VALUES ('1532', '420300000000', '420325000000', '房县');
INSERT INTO `j_position_county` VALUES ('1533', '420300000000', '420381000000', '丹江口市');
INSERT INTO `j_position_county` VALUES ('1534', '420500000000', '420502000000', '西陵区');
INSERT INTO `j_position_county` VALUES ('1535', '420500000000', '420503000000', '伍家岗区');
INSERT INTO `j_position_county` VALUES ('1536', '420500000000', '420504000000', '点军区');
INSERT INTO `j_position_county` VALUES ('1537', '420500000000', '420505000000', '猇亭区');
INSERT INTO `j_position_county` VALUES ('1538', '420500000000', '420506000000', '夷陵区');
INSERT INTO `j_position_county` VALUES ('1539', '420500000000', '420525000000', '远安县');
INSERT INTO `j_position_county` VALUES ('1540', '420500000000', '420526000000', '兴山县');
INSERT INTO `j_position_county` VALUES ('1541', '420500000000', '420527000000', '秭归县');
INSERT INTO `j_position_county` VALUES ('1542', '420500000000', '420528000000', '长阳土家族自治县');
INSERT INTO `j_position_county` VALUES ('1543', '420500000000', '420529000000', '五峰土家族自治县');
INSERT INTO `j_position_county` VALUES ('1544', '420500000000', '420581000000', '宜都市');
INSERT INTO `j_position_county` VALUES ('1545', '420500000000', '420582000000', '当阳市');
INSERT INTO `j_position_county` VALUES ('1546', '420500000000', '420583000000', '枝江市');
INSERT INTO `j_position_county` VALUES ('1547', '420600000000', '420602000000', '襄城区');
INSERT INTO `j_position_county` VALUES ('1548', '420600000000', '420606000000', '樊城区');
INSERT INTO `j_position_county` VALUES ('1549', '420600000000', '420607000000', '襄州区');
INSERT INTO `j_position_county` VALUES ('1550', '420600000000', '420624000000', '南漳县');
INSERT INTO `j_position_county` VALUES ('1551', '420600000000', '420625000000', '谷城县');
INSERT INTO `j_position_county` VALUES ('1552', '420600000000', '420626000000', '保康县');
INSERT INTO `j_position_county` VALUES ('1553', '420600000000', '420682000000', '老河口市');
INSERT INTO `j_position_county` VALUES ('1554', '420600000000', '420683000000', '枣阳市');
INSERT INTO `j_position_county` VALUES ('1555', '420600000000', '420684000000', '宜城市');
INSERT INTO `j_position_county` VALUES ('1556', '420700000000', '420702000000', '梁子湖区');
INSERT INTO `j_position_county` VALUES ('1557', '420700000000', '420703000000', '华容区');
INSERT INTO `j_position_county` VALUES ('1558', '420700000000', '420704000000', '鄂城区');
INSERT INTO `j_position_county` VALUES ('1559', '420800000000', '420802000000', '东宝区');
INSERT INTO `j_position_county` VALUES ('1560', '420800000000', '420804000000', '掇刀区');
INSERT INTO `j_position_county` VALUES ('1561', '420800000000', '420821000000', '京山县');
INSERT INTO `j_position_county` VALUES ('1562', '420800000000', '420822000000', '沙洋县');
INSERT INTO `j_position_county` VALUES ('1563', '420800000000', '420881000000', '钟祥市');
INSERT INTO `j_position_county` VALUES ('1564', '420900000000', '420902000000', '孝南区');
INSERT INTO `j_position_county` VALUES ('1565', '420900000000', '420921000000', '孝昌县');
INSERT INTO `j_position_county` VALUES ('1566', '420900000000', '420922000000', '大悟县');
INSERT INTO `j_position_county` VALUES ('1567', '420900000000', '420923000000', '云梦县');
INSERT INTO `j_position_county` VALUES ('1568', '420900000000', '420981000000', '应城市');
INSERT INTO `j_position_county` VALUES ('1569', '420900000000', '420982000000', '安陆市');
INSERT INTO `j_position_county` VALUES ('1570', '420900000000', '420984000000', '汉川市');
INSERT INTO `j_position_county` VALUES ('1571', '421000000000', '421002000000', '沙市区');
INSERT INTO `j_position_county` VALUES ('1572', '421000000000', '421003000000', '荆州区');
INSERT INTO `j_position_county` VALUES ('1573', '421000000000', '421022000000', '公安县');
INSERT INTO `j_position_county` VALUES ('1574', '421000000000', '421023000000', '监利县');
INSERT INTO `j_position_county` VALUES ('1575', '421000000000', '421024000000', '江陵县');
INSERT INTO `j_position_county` VALUES ('1576', '421000000000', '421081000000', '石首市');
INSERT INTO `j_position_county` VALUES ('1577', '421000000000', '421083000000', '洪湖市');
INSERT INTO `j_position_county` VALUES ('1578', '421000000000', '421087000000', '松滋市');
INSERT INTO `j_position_county` VALUES ('1579', '421100000000', '421102000000', '黄州区');
INSERT INTO `j_position_county` VALUES ('1580', '421100000000', '421121000000', '团风县');
INSERT INTO `j_position_county` VALUES ('1581', '421100000000', '421122000000', '红安县');
INSERT INTO `j_position_county` VALUES ('1582', '421100000000', '421123000000', '罗田县');
INSERT INTO `j_position_county` VALUES ('1583', '421100000000', '421124000000', '英山县');
INSERT INTO `j_position_county` VALUES ('1584', '421100000000', '421125000000', '浠水县');
INSERT INTO `j_position_county` VALUES ('1585', '421100000000', '421126000000', '蕲春县');
INSERT INTO `j_position_county` VALUES ('1586', '421100000000', '421127000000', '黄梅县');
INSERT INTO `j_position_county` VALUES ('1587', '421100000000', '421181000000', '麻城市');
INSERT INTO `j_position_county` VALUES ('1588', '421100000000', '421182000000', '武穴市');
INSERT INTO `j_position_county` VALUES ('1589', '421200000000', '421202000000', '咸安区');
INSERT INTO `j_position_county` VALUES ('1590', '421200000000', '421221000000', '嘉鱼县');
INSERT INTO `j_position_county` VALUES ('1591', '421200000000', '421222000000', '通城县');
INSERT INTO `j_position_county` VALUES ('1592', '421200000000', '421223000000', '崇阳县');
INSERT INTO `j_position_county` VALUES ('1593', '421200000000', '421224000000', '通山县');
INSERT INTO `j_position_county` VALUES ('1594', '421200000000', '421281000000', '赤壁市');
INSERT INTO `j_position_county` VALUES ('1595', '421300000000', '421303000000', '曾都区');
INSERT INTO `j_position_county` VALUES ('1596', '421300000000', '421321000000', '随县');
INSERT INTO `j_position_county` VALUES ('1597', '421300000000', '421381000000', '广水市');
INSERT INTO `j_position_county` VALUES ('1598', '422800000000', '422801000000', '恩施市');
INSERT INTO `j_position_county` VALUES ('1599', '422800000000', '422802000000', '利川市');
INSERT INTO `j_position_county` VALUES ('1600', '422800000000', '422822000000', '建始县');
INSERT INTO `j_position_county` VALUES ('1601', '422800000000', '422823000000', '巴东县');
INSERT INTO `j_position_county` VALUES ('1602', '422800000000', '422825000000', '宣恩县');
INSERT INTO `j_position_county` VALUES ('1603', '422800000000', '422826000000', '咸丰县');
INSERT INTO `j_position_county` VALUES ('1604', '422800000000', '422827000000', '来凤县');
INSERT INTO `j_position_county` VALUES ('1605', '422800000000', '422828000000', '鹤峰县');
INSERT INTO `j_position_county` VALUES ('1606', '429000000000', '429004000000', '仙桃市');
INSERT INTO `j_position_county` VALUES ('1607', '429000000000', '429005000000', '潜江市');
INSERT INTO `j_position_county` VALUES ('1608', '429000000000', '429006000000', '天门市');
INSERT INTO `j_position_county` VALUES ('1609', '429000000000', '429021000000', '神农架林区');
INSERT INTO `j_position_county` VALUES ('1610', '430100000000', '430102000000', '芙蓉区');
INSERT INTO `j_position_county` VALUES ('1611', '430100000000', '430103000000', '天心区');
INSERT INTO `j_position_county` VALUES ('1612', '430100000000', '430104000000', '岳麓区');
INSERT INTO `j_position_county` VALUES ('1613', '430100000000', '430105000000', '开福区');
INSERT INTO `j_position_county` VALUES ('1614', '430100000000', '430111000000', '雨花区');
INSERT INTO `j_position_county` VALUES ('1615', '430100000000', '430112000000', '望城区');
INSERT INTO `j_position_county` VALUES ('1616', '430100000000', '430121000000', '长沙县');
INSERT INTO `j_position_county` VALUES ('1617', '430100000000', '430124000000', '宁乡县');
INSERT INTO `j_position_county` VALUES ('1618', '430100000000', '430181000000', '浏阳市');
INSERT INTO `j_position_county` VALUES ('1619', '430200000000', '430202000000', '荷塘区');
INSERT INTO `j_position_county` VALUES ('1620', '430200000000', '430203000000', '芦淞区');
INSERT INTO `j_position_county` VALUES ('1621', '430200000000', '430204000000', '石峰区');
INSERT INTO `j_position_county` VALUES ('1622', '430200000000', '430211000000', '天元区');
INSERT INTO `j_position_county` VALUES ('1623', '430200000000', '430221000000', '株洲县');
INSERT INTO `j_position_county` VALUES ('1624', '430200000000', '430223000000', '攸县');
INSERT INTO `j_position_county` VALUES ('1625', '430200000000', '430224000000', '茶陵县');
INSERT INTO `j_position_county` VALUES ('1626', '430200000000', '430225000000', '炎陵县');
INSERT INTO `j_position_county` VALUES ('1627', '430200000000', '430281000000', '醴陵市');
INSERT INTO `j_position_county` VALUES ('1628', '430300000000', '430302000000', '雨湖区');
INSERT INTO `j_position_county` VALUES ('1629', '430300000000', '430304000000', '岳塘区');
INSERT INTO `j_position_county` VALUES ('1630', '430300000000', '430321000000', '湘潭县');
INSERT INTO `j_position_county` VALUES ('1631', '430300000000', '430381000000', '湘乡市');
INSERT INTO `j_position_county` VALUES ('1632', '430300000000', '430382000000', '韶山市');
INSERT INTO `j_position_county` VALUES ('1633', '430400000000', '430405000000', '珠晖区');
INSERT INTO `j_position_county` VALUES ('1634', '430400000000', '430406000000', '雁峰区');
INSERT INTO `j_position_county` VALUES ('1635', '430400000000', '430407000000', '石鼓区');
INSERT INTO `j_position_county` VALUES ('1636', '430400000000', '430408000000', '蒸湘区');
INSERT INTO `j_position_county` VALUES ('1637', '430400000000', '430412000000', '南岳区');
INSERT INTO `j_position_county` VALUES ('1638', '430400000000', '430421000000', '衡阳县');
INSERT INTO `j_position_county` VALUES ('1639', '430400000000', '430422000000', '衡南县');
INSERT INTO `j_position_county` VALUES ('1640', '430400000000', '430423000000', '衡山县');
INSERT INTO `j_position_county` VALUES ('1641', '430400000000', '430424000000', '衡东县');
INSERT INTO `j_position_county` VALUES ('1642', '430400000000', '430426000000', '祁东县');
INSERT INTO `j_position_county` VALUES ('1643', '430400000000', '430481000000', '耒阳市');
INSERT INTO `j_position_county` VALUES ('1644', '430400000000', '430482000000', '常宁市');
INSERT INTO `j_position_county` VALUES ('1645', '430500000000', '430502000000', '双清区');
INSERT INTO `j_position_county` VALUES ('1646', '430500000000', '430503000000', '大祥区');
INSERT INTO `j_position_county` VALUES ('1647', '430500000000', '430511000000', '北塔区');
INSERT INTO `j_position_county` VALUES ('1648', '430500000000', '430521000000', '邵东县');
INSERT INTO `j_position_county` VALUES ('1649', '430500000000', '430522000000', '新邵县');
INSERT INTO `j_position_county` VALUES ('1650', '430500000000', '430523000000', '邵阳县');
INSERT INTO `j_position_county` VALUES ('1651', '430500000000', '430524000000', '隆回县');
INSERT INTO `j_position_county` VALUES ('1652', '430500000000', '430525000000', '洞口县');
INSERT INTO `j_position_county` VALUES ('1653', '430500000000', '430527000000', '绥宁县');
INSERT INTO `j_position_county` VALUES ('1654', '430500000000', '430528000000', '新宁县');
INSERT INTO `j_position_county` VALUES ('1655', '430500000000', '430529000000', '城步苗族自治县');
INSERT INTO `j_position_county` VALUES ('1656', '430500000000', '430581000000', '武冈市');
INSERT INTO `j_position_county` VALUES ('1657', '430600000000', '430602000000', '岳阳楼区');
INSERT INTO `j_position_county` VALUES ('1658', '430600000000', '430603000000', '云溪区');
INSERT INTO `j_position_county` VALUES ('1659', '430600000000', '430611000000', '君山区');
INSERT INTO `j_position_county` VALUES ('1660', '430600000000', '430621000000', '岳阳县');
INSERT INTO `j_position_county` VALUES ('1661', '430600000000', '430623000000', '华容县');
INSERT INTO `j_position_county` VALUES ('1662', '430600000000', '430624000000', '湘阴县');
INSERT INTO `j_position_county` VALUES ('1663', '430600000000', '430626000000', '平江县');
INSERT INTO `j_position_county` VALUES ('1664', '430600000000', '430681000000', '汨罗市');
INSERT INTO `j_position_county` VALUES ('1665', '430600000000', '430682000000', '临湘市');
INSERT INTO `j_position_county` VALUES ('1666', '430700000000', '430702000000', '武陵区');
INSERT INTO `j_position_county` VALUES ('1667', '430700000000', '430703000000', '鼎城区');
INSERT INTO `j_position_county` VALUES ('1668', '430700000000', '430721000000', '安乡县');
INSERT INTO `j_position_county` VALUES ('1669', '430700000000', '430722000000', '汉寿县');
INSERT INTO `j_position_county` VALUES ('1670', '430700000000', '430723000000', '澧县');
INSERT INTO `j_position_county` VALUES ('1671', '430700000000', '430724000000', '临澧县');
INSERT INTO `j_position_county` VALUES ('1672', '430700000000', '430725000000', '桃源县');
INSERT INTO `j_position_county` VALUES ('1673', '430700000000', '430726000000', '石门县');
INSERT INTO `j_position_county` VALUES ('1674', '430700000000', '430781000000', '津市市');
INSERT INTO `j_position_county` VALUES ('1675', '430800000000', '430802000000', '永定区');
INSERT INTO `j_position_county` VALUES ('1676', '430800000000', '430811000000', '武陵源区');
INSERT INTO `j_position_county` VALUES ('1677', '430800000000', '430821000000', '慈利县');
INSERT INTO `j_position_county` VALUES ('1678', '430800000000', '430822000000', '桑植县');
INSERT INTO `j_position_county` VALUES ('1679', '430900000000', '430902000000', '资阳区');
INSERT INTO `j_position_county` VALUES ('1680', '430900000000', '430903000000', '赫山区');
INSERT INTO `j_position_county` VALUES ('1681', '430900000000', '430921000000', '南县');
INSERT INTO `j_position_county` VALUES ('1682', '430900000000', '430922000000', '桃江县');
INSERT INTO `j_position_county` VALUES ('1683', '430900000000', '430923000000', '安化县');
INSERT INTO `j_position_county` VALUES ('1684', '430900000000', '430981000000', '沅江市');
INSERT INTO `j_position_county` VALUES ('1685', '431000000000', '431002000000', '北湖区');
INSERT INTO `j_position_county` VALUES ('1686', '431000000000', '431003000000', '苏仙区');
INSERT INTO `j_position_county` VALUES ('1687', '431000000000', '431021000000', '桂阳县');
INSERT INTO `j_position_county` VALUES ('1688', '431000000000', '431022000000', '宜章县');
INSERT INTO `j_position_county` VALUES ('1689', '431000000000', '431023000000', '永兴县');
INSERT INTO `j_position_county` VALUES ('1690', '431000000000', '431024000000', '嘉禾县');
INSERT INTO `j_position_county` VALUES ('1691', '431000000000', '431025000000', '临武县');
INSERT INTO `j_position_county` VALUES ('1692', '431000000000', '431026000000', '汝城县');
INSERT INTO `j_position_county` VALUES ('1693', '431000000000', '431027000000', '桂东县');
INSERT INTO `j_position_county` VALUES ('1694', '431000000000', '431028000000', '安仁县');
INSERT INTO `j_position_county` VALUES ('1695', '431000000000', '431081000000', '资兴市');
INSERT INTO `j_position_county` VALUES ('1696', '431100000000', '431102000000', '零陵区');
INSERT INTO `j_position_county` VALUES ('1697', '431100000000', '431103000000', '冷水滩区');
INSERT INTO `j_position_county` VALUES ('1698', '431100000000', '431121000000', '祁阳县');
INSERT INTO `j_position_county` VALUES ('1699', '431100000000', '431122000000', '东安县');
INSERT INTO `j_position_county` VALUES ('1700', '431100000000', '431123000000', '双牌县');
INSERT INTO `j_position_county` VALUES ('1701', '431100000000', '431124000000', '道县');
INSERT INTO `j_position_county` VALUES ('1702', '431100000000', '431125000000', '江永县');
INSERT INTO `j_position_county` VALUES ('1703', '431100000000', '431126000000', '宁远县');
INSERT INTO `j_position_county` VALUES ('1704', '431100000000', '431127000000', '蓝山县');
INSERT INTO `j_position_county` VALUES ('1705', '431100000000', '431128000000', '新田县');
INSERT INTO `j_position_county` VALUES ('1706', '431100000000', '431129000000', '江华瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1707', '431200000000', '431202000000', '鹤城区');
INSERT INTO `j_position_county` VALUES ('1708', '431200000000', '431221000000', '中方县');
INSERT INTO `j_position_county` VALUES ('1709', '431200000000', '431222000000', '沅陵县');
INSERT INTO `j_position_county` VALUES ('1710', '431200000000', '431223000000', '辰溪县');
INSERT INTO `j_position_county` VALUES ('1711', '431200000000', '431224000000', '溆浦县');
INSERT INTO `j_position_county` VALUES ('1712', '431200000000', '431225000000', '会同县');
INSERT INTO `j_position_county` VALUES ('1713', '431200000000', '431226000000', '麻阳苗族自治县');
INSERT INTO `j_position_county` VALUES ('1714', '431200000000', '431227000000', '新晃侗族自治县');
INSERT INTO `j_position_county` VALUES ('1715', '431200000000', '431228000000', '芷江侗族自治县');
INSERT INTO `j_position_county` VALUES ('1716', '431200000000', '431229000000', '靖州苗族侗族自治县');
INSERT INTO `j_position_county` VALUES ('1717', '431200000000', '431230000000', '通道侗族自治县');
INSERT INTO `j_position_county` VALUES ('1718', '431200000000', '431281000000', '洪江市');
INSERT INTO `j_position_county` VALUES ('1719', '431300000000', '431302000000', '娄星区');
INSERT INTO `j_position_county` VALUES ('1720', '431300000000', '431321000000', '双峰县');
INSERT INTO `j_position_county` VALUES ('1721', '431300000000', '431322000000', '新化县');
INSERT INTO `j_position_county` VALUES ('1722', '431300000000', '431381000000', '冷水江市');
INSERT INTO `j_position_county` VALUES ('1723', '431300000000', '431382000000', '涟源市');
INSERT INTO `j_position_county` VALUES ('1724', '433100000000', '433101000000', '吉首市');
INSERT INTO `j_position_county` VALUES ('1725', '433100000000', '433122000000', '泸溪县');
INSERT INTO `j_position_county` VALUES ('1726', '433100000000', '433123000000', '凤凰县');
INSERT INTO `j_position_county` VALUES ('1727', '433100000000', '433124000000', '花垣县');
INSERT INTO `j_position_county` VALUES ('1728', '433100000000', '433125000000', '保靖县');
INSERT INTO `j_position_county` VALUES ('1729', '433100000000', '433126000000', '古丈县');
INSERT INTO `j_position_county` VALUES ('1730', '433100000000', '433127000000', '永顺县');
INSERT INTO `j_position_county` VALUES ('1731', '433100000000', '433130000000', '龙山县');
INSERT INTO `j_position_county` VALUES ('1732', '440100000000', '440103000000', '荔湾区');
INSERT INTO `j_position_county` VALUES ('1733', '440100000000', '440104000000', '越秀区');
INSERT INTO `j_position_county` VALUES ('1734', '440100000000', '440105000000', '海珠区');
INSERT INTO `j_position_county` VALUES ('1735', '440100000000', '440106000000', '天河区');
INSERT INTO `j_position_county` VALUES ('1736', '440100000000', '440111000000', '白云区');
INSERT INTO `j_position_county` VALUES ('1737', '440100000000', '440112000000', '黄埔区');
INSERT INTO `j_position_county` VALUES ('1738', '440100000000', '440113000000', '番禺区');
INSERT INTO `j_position_county` VALUES ('1739', '440100000000', '440114000000', '花都区');
INSERT INTO `j_position_county` VALUES ('1740', '440100000000', '440115000000', '南沙区');
INSERT INTO `j_position_county` VALUES ('1741', '440100000000', '440116000000', '萝岗区');
INSERT INTO `j_position_county` VALUES ('1742', '440100000000', '440183000000', '增城市');
INSERT INTO `j_position_county` VALUES ('1743', '440100000000', '440184000000', '从化市');
INSERT INTO `j_position_county` VALUES ('1744', '440200000000', '440203000000', '武江区');
INSERT INTO `j_position_county` VALUES ('1745', '440200000000', '440204000000', '浈江区');
INSERT INTO `j_position_county` VALUES ('1746', '440200000000', '440205000000', '曲江区');
INSERT INTO `j_position_county` VALUES ('1747', '440200000000', '440222000000', '始兴县');
INSERT INTO `j_position_county` VALUES ('1748', '440200000000', '440224000000', '仁化县');
INSERT INTO `j_position_county` VALUES ('1749', '440200000000', '440229000000', '翁源县');
INSERT INTO `j_position_county` VALUES ('1750', '440200000000', '440232000000', '乳源瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1751', '440200000000', '440233000000', '新丰县');
INSERT INTO `j_position_county` VALUES ('1752', '440200000000', '440281000000', '乐昌市');
INSERT INTO `j_position_county` VALUES ('1753', '440200000000', '440282000000', '南雄市');
INSERT INTO `j_position_county` VALUES ('1754', '440300000000', '440303000000', '罗湖区');
INSERT INTO `j_position_county` VALUES ('1755', '440300000000', '440304000000', '福田区');
INSERT INTO `j_position_county` VALUES ('1756', '440300000000', '440305000000', '南山区');
INSERT INTO `j_position_county` VALUES ('1757', '440300000000', '440306000000', '宝安区');
INSERT INTO `j_position_county` VALUES ('1758', '440300000000', '440307000000', '龙岗区');
INSERT INTO `j_position_county` VALUES ('1759', '440300000000', '440308000000', '盐田区');
INSERT INTO `j_position_county` VALUES ('1760', '440400000000', '440402000000', '香洲区');
INSERT INTO `j_position_county` VALUES ('1761', '440400000000', '440403000000', '斗门区');
INSERT INTO `j_position_county` VALUES ('1762', '440400000000', '440404000000', '金湾区');
INSERT INTO `j_position_county` VALUES ('1763', '440500000000', '440507000000', '龙湖区');
INSERT INTO `j_position_county` VALUES ('1764', '440500000000', '440511000000', '金平区');
INSERT INTO `j_position_county` VALUES ('1765', '440500000000', '440512000000', '濠江区');
INSERT INTO `j_position_county` VALUES ('1766', '440500000000', '440513000000', '潮阳区');
INSERT INTO `j_position_county` VALUES ('1767', '440500000000', '440514000000', '潮南区');
INSERT INTO `j_position_county` VALUES ('1768', '440500000000', '440515000000', '澄海区');
INSERT INTO `j_position_county` VALUES ('1769', '440500000000', '440523000000', '南澳县');
INSERT INTO `j_position_county` VALUES ('1770', '440600000000', '440604000000', '禅城区');
INSERT INTO `j_position_county` VALUES ('1771', '440600000000', '440605000000', '南海区');
INSERT INTO `j_position_county` VALUES ('1772', '440600000000', '440606000000', '顺德区');
INSERT INTO `j_position_county` VALUES ('1773', '440600000000', '440607000000', '三水区');
INSERT INTO `j_position_county` VALUES ('1774', '440600000000', '440608000000', '高明区');
INSERT INTO `j_position_county` VALUES ('1775', '440700000000', '440703000000', '蓬江区');
INSERT INTO `j_position_county` VALUES ('1776', '440700000000', '440704000000', '江海区');
INSERT INTO `j_position_county` VALUES ('1777', '440700000000', '440705000000', '新会区');
INSERT INTO `j_position_county` VALUES ('1778', '440700000000', '440781000000', '台山市');
INSERT INTO `j_position_county` VALUES ('1779', '440700000000', '440783000000', '开平市');
INSERT INTO `j_position_county` VALUES ('1780', '440700000000', '440784000000', '鹤山市');
INSERT INTO `j_position_county` VALUES ('1781', '440700000000', '440785000000', '恩平市');
INSERT INTO `j_position_county` VALUES ('1782', '440800000000', '440802000000', '赤坎区');
INSERT INTO `j_position_county` VALUES ('1783', '440800000000', '440803000000', '霞山区');
INSERT INTO `j_position_county` VALUES ('1784', '440800000000', '440804000000', '坡头区');
INSERT INTO `j_position_county` VALUES ('1785', '440800000000', '440811000000', '麻章区');
INSERT INTO `j_position_county` VALUES ('1786', '440800000000', '440823000000', '遂溪县');
INSERT INTO `j_position_county` VALUES ('1787', '440800000000', '440825000000', '徐闻县');
INSERT INTO `j_position_county` VALUES ('1788', '440800000000', '440881000000', '廉江市');
INSERT INTO `j_position_county` VALUES ('1789', '440800000000', '440882000000', '雷州市');
INSERT INTO `j_position_county` VALUES ('1790', '440800000000', '440883000000', '吴川市');
INSERT INTO `j_position_county` VALUES ('1791', '440900000000', '440902000000', '茂南区');
INSERT INTO `j_position_county` VALUES ('1792', '440900000000', '440903000000', '茂港区');
INSERT INTO `j_position_county` VALUES ('1793', '440900000000', '440923000000', '电白县');
INSERT INTO `j_position_county` VALUES ('1794', '440900000000', '440981000000', '高州市');
INSERT INTO `j_position_county` VALUES ('1795', '440900000000', '440982000000', '化州市');
INSERT INTO `j_position_county` VALUES ('1796', '440900000000', '440983000000', '信宜市');
INSERT INTO `j_position_county` VALUES ('1797', '441200000000', '441202000000', '端州区');
INSERT INTO `j_position_county` VALUES ('1798', '441200000000', '441203000000', '鼎湖区');
INSERT INTO `j_position_county` VALUES ('1799', '441200000000', '441223000000', '广宁县');
INSERT INTO `j_position_county` VALUES ('1800', '441200000000', '441224000000', '怀集县');
INSERT INTO `j_position_county` VALUES ('1801', '441200000000', '441225000000', '封开县');
INSERT INTO `j_position_county` VALUES ('1802', '441200000000', '441226000000', '德庆县');
INSERT INTO `j_position_county` VALUES ('1803', '441200000000', '441283000000', '高要市');
INSERT INTO `j_position_county` VALUES ('1804', '441200000000', '441284000000', '四会市');
INSERT INTO `j_position_county` VALUES ('1805', '441300000000', '441302000000', '惠城区');
INSERT INTO `j_position_county` VALUES ('1806', '441300000000', '441303000000', '惠阳区');
INSERT INTO `j_position_county` VALUES ('1807', '441300000000', '441322000000', '博罗县');
INSERT INTO `j_position_county` VALUES ('1808', '441300000000', '441323000000', '惠东县');
INSERT INTO `j_position_county` VALUES ('1809', '441300000000', '441324000000', '龙门县');
INSERT INTO `j_position_county` VALUES ('1810', '441400000000', '441402000000', '梅江区');
INSERT INTO `j_position_county` VALUES ('1811', '441400000000', '441421000000', '梅县');
INSERT INTO `j_position_county` VALUES ('1812', '441400000000', '441422000000', '大埔县');
INSERT INTO `j_position_county` VALUES ('1813', '441400000000', '441423000000', '丰顺县');
INSERT INTO `j_position_county` VALUES ('1814', '441400000000', '441424000000', '五华县');
INSERT INTO `j_position_county` VALUES ('1815', '441400000000', '441426000000', '平远县');
INSERT INTO `j_position_county` VALUES ('1816', '441400000000', '441427000000', '蕉岭县');
INSERT INTO `j_position_county` VALUES ('1817', '441400000000', '441481000000', '兴宁市');
INSERT INTO `j_position_county` VALUES ('1818', '441500000000', '441502000000', '城区');
INSERT INTO `j_position_county` VALUES ('1819', '441500000000', '441521000000', '海丰县');
INSERT INTO `j_position_county` VALUES ('1820', '441500000000', '441523000000', '陆河县');
INSERT INTO `j_position_county` VALUES ('1821', '441500000000', '441581000000', '陆丰市');
INSERT INTO `j_position_county` VALUES ('1822', '441600000000', '441602000000', '源城区');
INSERT INTO `j_position_county` VALUES ('1823', '441600000000', '441621000000', '紫金县');
INSERT INTO `j_position_county` VALUES ('1824', '441600000000', '441622000000', '龙川县');
INSERT INTO `j_position_county` VALUES ('1825', '441600000000', '441623000000', '连平县');
INSERT INTO `j_position_county` VALUES ('1826', '441600000000', '441624000000', '和平县');
INSERT INTO `j_position_county` VALUES ('1827', '441600000000', '441625000000', '东源县');
INSERT INTO `j_position_county` VALUES ('1828', '441700000000', '441702000000', '江城区');
INSERT INTO `j_position_county` VALUES ('1829', '441700000000', '441721000000', '阳西县');
INSERT INTO `j_position_county` VALUES ('1830', '441700000000', '441723000000', '阳东县');
INSERT INTO `j_position_county` VALUES ('1831', '441700000000', '441781000000', '阳春市');
INSERT INTO `j_position_county` VALUES ('1832', '441800000000', '441802000000', '清城区');
INSERT INTO `j_position_county` VALUES ('1833', '441800000000', '441803000000', '清新区');
INSERT INTO `j_position_county` VALUES ('1834', '441800000000', '441821000000', '佛冈县');
INSERT INTO `j_position_county` VALUES ('1835', '441800000000', '441823000000', '阳山县');
INSERT INTO `j_position_county` VALUES ('1836', '441800000000', '441825000000', '连山壮族瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1837', '441800000000', '441826000000', '连南瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1838', '441800000000', '441881000000', '英德市');
INSERT INTO `j_position_county` VALUES ('1839', '441800000000', '441882000000', '连州市');
INSERT INTO `j_position_county` VALUES ('1840', '445100000000', '445102000000', '湘桥区');
INSERT INTO `j_position_county` VALUES ('1841', '445100000000', '445103000000', '潮安区');
INSERT INTO `j_position_county` VALUES ('1842', '445100000000', '445122000000', '饶平县');
INSERT INTO `j_position_county` VALUES ('1843', '445200000000', '445202000000', '榕城区');
INSERT INTO `j_position_county` VALUES ('1844', '445200000000', '445203000000', '揭东区');
INSERT INTO `j_position_county` VALUES ('1845', '445200000000', '445222000000', '揭西县');
INSERT INTO `j_position_county` VALUES ('1846', '445200000000', '445224000000', '惠来县');
INSERT INTO `j_position_county` VALUES ('1847', '445200000000', '445281000000', '普宁市');
INSERT INTO `j_position_county` VALUES ('1848', '445300000000', '445302000000', '云城区');
INSERT INTO `j_position_county` VALUES ('1849', '445300000000', '445321000000', '新兴县');
INSERT INTO `j_position_county` VALUES ('1850', '445300000000', '445322000000', '郁南县');
INSERT INTO `j_position_county` VALUES ('1851', '445300000000', '445323000000', '云安县');
INSERT INTO `j_position_county` VALUES ('1852', '445300000000', '445381000000', '罗定市');
INSERT INTO `j_position_county` VALUES ('1853', '450100000000', '450102000000', '兴宁区');
INSERT INTO `j_position_county` VALUES ('1854', '450100000000', '450103000000', '青秀区');
INSERT INTO `j_position_county` VALUES ('1855', '450100000000', '450105000000', '江南区');
INSERT INTO `j_position_county` VALUES ('1856', '450100000000', '450107000000', '西乡塘区');
INSERT INTO `j_position_county` VALUES ('1857', '450100000000', '450108000000', '良庆区');
INSERT INTO `j_position_county` VALUES ('1858', '450100000000', '450109000000', '邕宁区');
INSERT INTO `j_position_county` VALUES ('1859', '450100000000', '450122000000', '武鸣县');
INSERT INTO `j_position_county` VALUES ('1860', '450100000000', '450123000000', '隆安县');
INSERT INTO `j_position_county` VALUES ('1861', '450100000000', '450124000000', '马山县');
INSERT INTO `j_position_county` VALUES ('1862', '450100000000', '450125000000', '上林县');
INSERT INTO `j_position_county` VALUES ('1863', '450100000000', '450126000000', '宾阳县');
INSERT INTO `j_position_county` VALUES ('1864', '450100000000', '450127000000', '横县');
INSERT INTO `j_position_county` VALUES ('1865', '450200000000', '450202000000', '城中区');
INSERT INTO `j_position_county` VALUES ('1866', '450200000000', '450203000000', '鱼峰区');
INSERT INTO `j_position_county` VALUES ('1867', '450200000000', '450204000000', '柳南区');
INSERT INTO `j_position_county` VALUES ('1868', '450200000000', '450205000000', '柳北区');
INSERT INTO `j_position_county` VALUES ('1869', '450200000000', '450221000000', '柳江县');
INSERT INTO `j_position_county` VALUES ('1870', '450200000000', '450222000000', '柳城县');
INSERT INTO `j_position_county` VALUES ('1871', '450200000000', '450223000000', '鹿寨县');
INSERT INTO `j_position_county` VALUES ('1872', '450200000000', '450224000000', '融安县');
INSERT INTO `j_position_county` VALUES ('1873', '450200000000', '450225000000', '融水苗族自治县');
INSERT INTO `j_position_county` VALUES ('1874', '450200000000', '450226000000', '三江侗族自治县');
INSERT INTO `j_position_county` VALUES ('1875', '450300000000', '450302000000', '秀峰区');
INSERT INTO `j_position_county` VALUES ('1876', '450300000000', '450303000000', '叠彩区');
INSERT INTO `j_position_county` VALUES ('1877', '450300000000', '450304000000', '象山区');
INSERT INTO `j_position_county` VALUES ('1878', '450300000000', '450305000000', '七星区');
INSERT INTO `j_position_county` VALUES ('1879', '450300000000', '450311000000', '雁山区');
INSERT INTO `j_position_county` VALUES ('1880', '450300000000', '450312000000', '临桂区');
INSERT INTO `j_position_county` VALUES ('1881', '450300000000', '450321000000', '阳朔县');
INSERT INTO `j_position_county` VALUES ('1882', '450300000000', '450323000000', '灵川县');
INSERT INTO `j_position_county` VALUES ('1883', '450300000000', '450324000000', '全州县');
INSERT INTO `j_position_county` VALUES ('1884', '450300000000', '450325000000', '兴安县');
INSERT INTO `j_position_county` VALUES ('1885', '450300000000', '450326000000', '永福县');
INSERT INTO `j_position_county` VALUES ('1886', '450300000000', '450327000000', '灌阳县');
INSERT INTO `j_position_county` VALUES ('1887', '450300000000', '450328000000', '龙胜各族自治县');
INSERT INTO `j_position_county` VALUES ('1888', '450300000000', '450329000000', '资源县');
INSERT INTO `j_position_county` VALUES ('1889', '450300000000', '450330000000', '平乐县');
INSERT INTO `j_position_county` VALUES ('1890', '450300000000', '450331000000', '荔浦县');
INSERT INTO `j_position_county` VALUES ('1891', '450300000000', '450332000000', '恭城瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1892', '450400000000', '450403000000', '万秀区');
INSERT INTO `j_position_county` VALUES ('1893', '450400000000', '450405000000', '长洲区');
INSERT INTO `j_position_county` VALUES ('1894', '450400000000', '450406000000', '龙圩区');
INSERT INTO `j_position_county` VALUES ('1895', '450400000000', '450421000000', '苍梧县');
INSERT INTO `j_position_county` VALUES ('1896', '450400000000', '450422000000', '藤县');
INSERT INTO `j_position_county` VALUES ('1897', '450400000000', '450423000000', '蒙山县');
INSERT INTO `j_position_county` VALUES ('1898', '450400000000', '450481000000', '岑溪市');
INSERT INTO `j_position_county` VALUES ('1899', '450500000000', '450502000000', '海城区');
INSERT INTO `j_position_county` VALUES ('1900', '450500000000', '450503000000', '银海区');
INSERT INTO `j_position_county` VALUES ('1901', '450500000000', '450512000000', '铁山港区');
INSERT INTO `j_position_county` VALUES ('1902', '450500000000', '450521000000', '合浦县');
INSERT INTO `j_position_county` VALUES ('1903', '450600000000', '450602000000', '港口区');
INSERT INTO `j_position_county` VALUES ('1904', '450600000000', '450603000000', '防城区');
INSERT INTO `j_position_county` VALUES ('1905', '450600000000', '450621000000', '上思县');
INSERT INTO `j_position_county` VALUES ('1906', '450600000000', '450681000000', '东兴市');
INSERT INTO `j_position_county` VALUES ('1907', '450700000000', '450702000000', '钦南区');
INSERT INTO `j_position_county` VALUES ('1908', '450700000000', '450703000000', '钦北区');
INSERT INTO `j_position_county` VALUES ('1909', '450700000000', '450721000000', '灵山县');
INSERT INTO `j_position_county` VALUES ('1910', '450700000000', '450722000000', '浦北县');
INSERT INTO `j_position_county` VALUES ('1911', '450800000000', '450802000000', '港北区');
INSERT INTO `j_position_county` VALUES ('1912', '450800000000', '450803000000', '港南区');
INSERT INTO `j_position_county` VALUES ('1913', '450800000000', '450804000000', '覃塘区');
INSERT INTO `j_position_county` VALUES ('1914', '450800000000', '450821000000', '平南县');
INSERT INTO `j_position_county` VALUES ('1915', '450800000000', '450881000000', '桂平市');
INSERT INTO `j_position_county` VALUES ('1916', '450900000000', '450902000000', '玉州区');
INSERT INTO `j_position_county` VALUES ('1917', '450900000000', '450903000000', '福绵区');
INSERT INTO `j_position_county` VALUES ('1918', '450900000000', '450921000000', '容县');
INSERT INTO `j_position_county` VALUES ('1919', '450900000000', '450922000000', '陆川县');
INSERT INTO `j_position_county` VALUES ('1920', '450900000000', '450923000000', '博白县');
INSERT INTO `j_position_county` VALUES ('1921', '450900000000', '450924000000', '兴业县');
INSERT INTO `j_position_county` VALUES ('1922', '450900000000', '450981000000', '北流市');
INSERT INTO `j_position_county` VALUES ('1923', '451000000000', '451002000000', '右江区');
INSERT INTO `j_position_county` VALUES ('1924', '451000000000', '451021000000', '田阳县');
INSERT INTO `j_position_county` VALUES ('1925', '451000000000', '451022000000', '田东县');
INSERT INTO `j_position_county` VALUES ('1926', '451000000000', '451023000000', '平果县');
INSERT INTO `j_position_county` VALUES ('1927', '451000000000', '451024000000', '德保县');
INSERT INTO `j_position_county` VALUES ('1928', '451000000000', '451025000000', '靖西县');
INSERT INTO `j_position_county` VALUES ('1929', '451000000000', '451026000000', '那坡县');
INSERT INTO `j_position_county` VALUES ('1930', '451000000000', '451027000000', '凌云县');
INSERT INTO `j_position_county` VALUES ('1931', '451000000000', '451028000000', '乐业县');
INSERT INTO `j_position_county` VALUES ('1932', '451000000000', '451029000000', '田林县');
INSERT INTO `j_position_county` VALUES ('1933', '451000000000', '451030000000', '西林县');
INSERT INTO `j_position_county` VALUES ('1934', '451000000000', '451031000000', '隆林各族自治县');
INSERT INTO `j_position_county` VALUES ('1935', '451100000000', '451102000000', '八步区');
INSERT INTO `j_position_county` VALUES ('1936', '451100000000', '451121000000', '昭平县');
INSERT INTO `j_position_county` VALUES ('1937', '451100000000', '451122000000', '钟山县');
INSERT INTO `j_position_county` VALUES ('1938', '451100000000', '451123000000', '富川瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1939', '451200000000', '451202000000', '金城江区');
INSERT INTO `j_position_county` VALUES ('1940', '451200000000', '451221000000', '南丹县');
INSERT INTO `j_position_county` VALUES ('1941', '451200000000', '451222000000', '天峨县');
INSERT INTO `j_position_county` VALUES ('1942', '451200000000', '451223000000', '凤山县');
INSERT INTO `j_position_county` VALUES ('1943', '451200000000', '451224000000', '东兰县');
INSERT INTO `j_position_county` VALUES ('1944', '451200000000', '451225000000', '罗城仫佬族自治县');
INSERT INTO `j_position_county` VALUES ('1945', '451200000000', '451226000000', '环江毛南族自治县');
INSERT INTO `j_position_county` VALUES ('1946', '451200000000', '451227000000', '巴马瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1947', '451200000000', '451228000000', '都安瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1948', '451200000000', '451229000000', '大化瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1949', '451200000000', '451281000000', '宜州市');
INSERT INTO `j_position_county` VALUES ('1950', '451300000000', '451302000000', '兴宾区');
INSERT INTO `j_position_county` VALUES ('1951', '451300000000', '451321000000', '忻城县');
INSERT INTO `j_position_county` VALUES ('1952', '451300000000', '451322000000', '象州县');
INSERT INTO `j_position_county` VALUES ('1953', '451300000000', '451323000000', '武宣县');
INSERT INTO `j_position_county` VALUES ('1954', '451300000000', '451324000000', '金秀瑶族自治县');
INSERT INTO `j_position_county` VALUES ('1955', '451300000000', '451381000000', '合山市');
INSERT INTO `j_position_county` VALUES ('1956', '451400000000', '451402000000', '江州区');
INSERT INTO `j_position_county` VALUES ('1957', '451400000000', '451421000000', '扶绥县');
INSERT INTO `j_position_county` VALUES ('1958', '451400000000', '451422000000', '宁明县');
INSERT INTO `j_position_county` VALUES ('1959', '451400000000', '451423000000', '龙州县');
INSERT INTO `j_position_county` VALUES ('1960', '451400000000', '451424000000', '大新县');
INSERT INTO `j_position_county` VALUES ('1961', '451400000000', '451425000000', '天等县');
INSERT INTO `j_position_county` VALUES ('1962', '451400000000', '451481000000', '凭祥市');
INSERT INTO `j_position_county` VALUES ('1963', '460100000000', '460105000000', '秀英区');
INSERT INTO `j_position_county` VALUES ('1964', '460100000000', '460106000000', '龙华区');
INSERT INTO `j_position_county` VALUES ('1965', '460100000000', '460107000000', '琼山区');
INSERT INTO `j_position_county` VALUES ('1966', '460100000000', '460108000000', '美兰区');
INSERT INTO `j_position_county` VALUES ('1967', '460200000000', '460201000000', '市辖区');
INSERT INTO `j_position_county` VALUES ('1968', '460300000000', '460321000000', '西沙群岛');
INSERT INTO `j_position_county` VALUES ('1969', '460300000000', '460322000000', '南沙群岛');
INSERT INTO `j_position_county` VALUES ('1970', '460300000000', '460323000000', '中沙群岛的岛礁及其海域');
INSERT INTO `j_position_county` VALUES ('1971', '469000000000', '469001000000', '五指山市');
INSERT INTO `j_position_county` VALUES ('1972', '469000000000', '469002000000', '琼海市');
INSERT INTO `j_position_county` VALUES ('1973', '469000000000', '469003000000', '儋州市');
INSERT INTO `j_position_county` VALUES ('1974', '469000000000', '469005000000', '文昌市');
INSERT INTO `j_position_county` VALUES ('1975', '469000000000', '469006000000', '万宁市');
INSERT INTO `j_position_county` VALUES ('1976', '469000000000', '469007000000', '东方市');
INSERT INTO `j_position_county` VALUES ('1977', '469000000000', '469021000000', '定安县');
INSERT INTO `j_position_county` VALUES ('1978', '469000000000', '469022000000', '屯昌县');
INSERT INTO `j_position_county` VALUES ('1979', '469000000000', '469023000000', '澄迈县');
INSERT INTO `j_position_county` VALUES ('1980', '469000000000', '469024000000', '临高县');
INSERT INTO `j_position_county` VALUES ('1981', '469000000000', '469025000000', '白沙黎族自治县');
INSERT INTO `j_position_county` VALUES ('1982', '469000000000', '469026000000', '昌江黎族自治县');
INSERT INTO `j_position_county` VALUES ('1983', '469000000000', '469027000000', '乐东黎族自治县');
INSERT INTO `j_position_county` VALUES ('1984', '469000000000', '469028000000', '陵水黎族自治县');
INSERT INTO `j_position_county` VALUES ('1985', '469000000000', '469029000000', '保亭黎族苗族自治县');
INSERT INTO `j_position_county` VALUES ('1986', '469000000000', '469030000000', '琼中黎族苗族自治县');
INSERT INTO `j_position_county` VALUES ('1987', '500100000000', '500101000000', '万州区');
INSERT INTO `j_position_county` VALUES ('1988', '500100000000', '500102000000', '涪陵区');
INSERT INTO `j_position_county` VALUES ('1989', '500100000000', '500103000000', '渝中区');
INSERT INTO `j_position_county` VALUES ('1990', '500100000000', '500104000000', '大渡口区');
INSERT INTO `j_position_county` VALUES ('1991', '500100000000', '500105000000', '江北区');
INSERT INTO `j_position_county` VALUES ('1992', '500100000000', '500106000000', '沙坪坝区');
INSERT INTO `j_position_county` VALUES ('1993', '500100000000', '500107000000', '九龙坡区');
INSERT INTO `j_position_county` VALUES ('1994', '500100000000', '500108000000', '南岸区');
INSERT INTO `j_position_county` VALUES ('1995', '500100000000', '500109000000', '北碚区');
INSERT INTO `j_position_county` VALUES ('1996', '500100000000', '500110000000', '綦江区');
INSERT INTO `j_position_county` VALUES ('1997', '500100000000', '500111000000', '大足区');
INSERT INTO `j_position_county` VALUES ('1998', '500100000000', '500112000000', '渝北区');
INSERT INTO `j_position_county` VALUES ('1999', '500100000000', '500113000000', '巴南区');
INSERT INTO `j_position_county` VALUES ('2000', '500100000000', '500114000000', '黔江区');
INSERT INTO `j_position_county` VALUES ('2001', '500100000000', '500115000000', '长寿区');
INSERT INTO `j_position_county` VALUES ('2002', '500100000000', '500116000000', '江津区');
INSERT INTO `j_position_county` VALUES ('2003', '500100000000', '500117000000', '合川区');
INSERT INTO `j_position_county` VALUES ('2004', '500100000000', '500118000000', '永川区');
INSERT INTO `j_position_county` VALUES ('2005', '500100000000', '500119000000', '南川区');
INSERT INTO `j_position_county` VALUES ('2006', '500200000000', '500223000000', '潼南县');
INSERT INTO `j_position_county` VALUES ('2007', '500200000000', '500224000000', '铜梁县');
INSERT INTO `j_position_county` VALUES ('2008', '500200000000', '500226000000', '荣昌县');
INSERT INTO `j_position_county` VALUES ('2009', '500200000000', '500227000000', '璧山县');
INSERT INTO `j_position_county` VALUES ('2010', '500200000000', '500228000000', '梁平县');
INSERT INTO `j_position_county` VALUES ('2011', '500200000000', '500229000000', '城口县');
INSERT INTO `j_position_county` VALUES ('2012', '500200000000', '500230000000', '丰都县');
INSERT INTO `j_position_county` VALUES ('2013', '500200000000', '500231000000', '垫江县');
INSERT INTO `j_position_county` VALUES ('2014', '500200000000', '500232000000', '武隆县');
INSERT INTO `j_position_county` VALUES ('2015', '500200000000', '500233000000', '忠县');
INSERT INTO `j_position_county` VALUES ('2016', '500200000000', '500234000000', '开县');
INSERT INTO `j_position_county` VALUES ('2017', '500200000000', '500235000000', '云阳县');
INSERT INTO `j_position_county` VALUES ('2018', '500200000000', '500236000000', '奉节县');
INSERT INTO `j_position_county` VALUES ('2019', '500200000000', '500237000000', '巫山县');
INSERT INTO `j_position_county` VALUES ('2020', '500200000000', '500238000000', '巫溪县');
INSERT INTO `j_position_county` VALUES ('2021', '500200000000', '500240000000', '石柱土家族自治县');
INSERT INTO `j_position_county` VALUES ('2022', '500200000000', '500241000000', '秀山土家族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2023', '500200000000', '500242000000', '酉阳土家族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2024', '500200000000', '500243000000', '彭水苗族土家族自治县');
INSERT INTO `j_position_county` VALUES ('2025', '510100000000', '510104000000', '锦江区');
INSERT INTO `j_position_county` VALUES ('2026', '510100000000', '510105000000', '青羊区');
INSERT INTO `j_position_county` VALUES ('2027', '510100000000', '510106000000', '金牛区');
INSERT INTO `j_position_county` VALUES ('2028', '510100000000', '510107000000', '武侯区');
INSERT INTO `j_position_county` VALUES ('2029', '510100000000', '510108000000', '成华区');
INSERT INTO `j_position_county` VALUES ('2030', '510100000000', '510112000000', '龙泉驿区');
INSERT INTO `j_position_county` VALUES ('2031', '510100000000', '510113000000', '青白江区');
INSERT INTO `j_position_county` VALUES ('2032', '510100000000', '510114000000', '新都区');
INSERT INTO `j_position_county` VALUES ('2033', '510100000000', '510115000000', '温江区');
INSERT INTO `j_position_county` VALUES ('2034', '510100000000', '510121000000', '金堂县');
INSERT INTO `j_position_county` VALUES ('2035', '510100000000', '510122000000', '双流县');
INSERT INTO `j_position_county` VALUES ('2036', '510100000000', '510124000000', '郫县');
INSERT INTO `j_position_county` VALUES ('2037', '510100000000', '510129000000', '大邑县');
INSERT INTO `j_position_county` VALUES ('2038', '510100000000', '510131000000', '蒲江县');
INSERT INTO `j_position_county` VALUES ('2039', '510100000000', '510132000000', '新津县');
INSERT INTO `j_position_county` VALUES ('2040', '510100000000', '510181000000', '都江堰市');
INSERT INTO `j_position_county` VALUES ('2041', '510100000000', '510182000000', '彭州市');
INSERT INTO `j_position_county` VALUES ('2042', '510100000000', '510183000000', '邛崃市');
INSERT INTO `j_position_county` VALUES ('2043', '510100000000', '510184000000', '崇州市');
INSERT INTO `j_position_county` VALUES ('2044', '510300000000', '510302000000', '自流井区');
INSERT INTO `j_position_county` VALUES ('2045', '510300000000', '510303000000', '贡井区');
INSERT INTO `j_position_county` VALUES ('2046', '510300000000', '510304000000', '大安区');
INSERT INTO `j_position_county` VALUES ('2047', '510300000000', '510311000000', '沿滩区');
INSERT INTO `j_position_county` VALUES ('2048', '510300000000', '510321000000', '荣县');
INSERT INTO `j_position_county` VALUES ('2049', '510300000000', '510322000000', '富顺县');
INSERT INTO `j_position_county` VALUES ('2050', '510400000000', '510402000000', '东区');
INSERT INTO `j_position_county` VALUES ('2051', '510400000000', '510403000000', '西区');
INSERT INTO `j_position_county` VALUES ('2052', '510400000000', '510411000000', '仁和区');
INSERT INTO `j_position_county` VALUES ('2053', '510400000000', '510421000000', '米易县');
INSERT INTO `j_position_county` VALUES ('2054', '510400000000', '510422000000', '盐边县');
INSERT INTO `j_position_county` VALUES ('2055', '510500000000', '510502000000', '江阳区');
INSERT INTO `j_position_county` VALUES ('2056', '510500000000', '510503000000', '纳溪区');
INSERT INTO `j_position_county` VALUES ('2057', '510500000000', '510504000000', '龙马潭区');
INSERT INTO `j_position_county` VALUES ('2058', '510500000000', '510521000000', '泸县');
INSERT INTO `j_position_county` VALUES ('2059', '510500000000', '510522000000', '合江县');
INSERT INTO `j_position_county` VALUES ('2060', '510500000000', '510524000000', '叙永县');
INSERT INTO `j_position_county` VALUES ('2061', '510500000000', '510525000000', '古蔺县');
INSERT INTO `j_position_county` VALUES ('2062', '510600000000', '510603000000', '旌阳区');
INSERT INTO `j_position_county` VALUES ('2063', '510600000000', '510623000000', '中江县');
INSERT INTO `j_position_county` VALUES ('2064', '510600000000', '510626000000', '罗江县');
INSERT INTO `j_position_county` VALUES ('2065', '510600000000', '510681000000', '广汉市');
INSERT INTO `j_position_county` VALUES ('2066', '510600000000', '510682000000', '什邡市');
INSERT INTO `j_position_county` VALUES ('2067', '510600000000', '510683000000', '绵竹市');
INSERT INTO `j_position_county` VALUES ('2068', '510700000000', '510703000000', '涪城区');
INSERT INTO `j_position_county` VALUES ('2069', '510700000000', '510704000000', '游仙区');
INSERT INTO `j_position_county` VALUES ('2070', '510700000000', '510722000000', '三台县');
INSERT INTO `j_position_county` VALUES ('2071', '510700000000', '510723000000', '盐亭县');
INSERT INTO `j_position_county` VALUES ('2072', '510700000000', '510724000000', '安县');
INSERT INTO `j_position_county` VALUES ('2073', '510700000000', '510725000000', '梓潼县');
INSERT INTO `j_position_county` VALUES ('2074', '510700000000', '510726000000', '北川羌族自治县');
INSERT INTO `j_position_county` VALUES ('2075', '510700000000', '510727000000', '平武县');
INSERT INTO `j_position_county` VALUES ('2076', '510700000000', '510781000000', '江油市');
INSERT INTO `j_position_county` VALUES ('2077', '510800000000', '510802000000', '利州区');
INSERT INTO `j_position_county` VALUES ('2078', '510800000000', '510811000000', '元坝区');
INSERT INTO `j_position_county` VALUES ('2079', '510800000000', '510812000000', '朝天区');
INSERT INTO `j_position_county` VALUES ('2080', '510800000000', '510821000000', '旺苍县');
INSERT INTO `j_position_county` VALUES ('2081', '510800000000', '510822000000', '青川县');
INSERT INTO `j_position_county` VALUES ('2082', '510800000000', '510823000000', '剑阁县');
INSERT INTO `j_position_county` VALUES ('2083', '510800000000', '510824000000', '苍溪县');
INSERT INTO `j_position_county` VALUES ('2084', '510900000000', '510903000000', '船山区');
INSERT INTO `j_position_county` VALUES ('2085', '510900000000', '510904000000', '安居区');
INSERT INTO `j_position_county` VALUES ('2086', '510900000000', '510921000000', '蓬溪县');
INSERT INTO `j_position_county` VALUES ('2087', '510900000000', '510922000000', '射洪县');
INSERT INTO `j_position_county` VALUES ('2088', '510900000000', '510923000000', '大英县');
INSERT INTO `j_position_county` VALUES ('2089', '511000000000', '511002000000', '市中区');
INSERT INTO `j_position_county` VALUES ('2090', '511000000000', '511011000000', '东兴区');
INSERT INTO `j_position_county` VALUES ('2091', '511000000000', '511024000000', '威远县');
INSERT INTO `j_position_county` VALUES ('2092', '511000000000', '511025000000', '资中县');
INSERT INTO `j_position_county` VALUES ('2093', '511000000000', '511028000000', '隆昌县');
INSERT INTO `j_position_county` VALUES ('2094', '511100000000', '511102000000', '市中区');
INSERT INTO `j_position_county` VALUES ('2095', '511100000000', '511111000000', '沙湾区');
INSERT INTO `j_position_county` VALUES ('2096', '511100000000', '511112000000', '五通桥区');
INSERT INTO `j_position_county` VALUES ('2097', '511100000000', '511113000000', '金口河区');
INSERT INTO `j_position_county` VALUES ('2098', '511100000000', '511123000000', '犍为县');
INSERT INTO `j_position_county` VALUES ('2099', '511100000000', '511124000000', '井研县');
INSERT INTO `j_position_county` VALUES ('2100', '511100000000', '511126000000', '夹江县');
INSERT INTO `j_position_county` VALUES ('2101', '511100000000', '511129000000', '沐川县');
INSERT INTO `j_position_county` VALUES ('2102', '511100000000', '511132000000', '峨边彝族自治县');
INSERT INTO `j_position_county` VALUES ('2103', '511100000000', '511133000000', '马边彝族自治县');
INSERT INTO `j_position_county` VALUES ('2104', '511100000000', '511181000000', '峨眉山市');
INSERT INTO `j_position_county` VALUES ('2105', '511300000000', '511302000000', '顺庆区');
INSERT INTO `j_position_county` VALUES ('2106', '511300000000', '511303000000', '高坪区');
INSERT INTO `j_position_county` VALUES ('2107', '511300000000', '511304000000', '嘉陵区');
INSERT INTO `j_position_county` VALUES ('2108', '511300000000', '511321000000', '南部县');
INSERT INTO `j_position_county` VALUES ('2109', '511300000000', '511322000000', '营山县');
INSERT INTO `j_position_county` VALUES ('2110', '511300000000', '511323000000', '蓬安县');
INSERT INTO `j_position_county` VALUES ('2111', '511300000000', '511324000000', '仪陇县');
INSERT INTO `j_position_county` VALUES ('2112', '511300000000', '511325000000', '西充县');
INSERT INTO `j_position_county` VALUES ('2113', '511300000000', '511381000000', '阆中市');
INSERT INTO `j_position_county` VALUES ('2114', '511400000000', '511402000000', '东坡区');
INSERT INTO `j_position_county` VALUES ('2115', '511400000000', '511421000000', '仁寿县');
INSERT INTO `j_position_county` VALUES ('2116', '511400000000', '511422000000', '彭山县');
INSERT INTO `j_position_county` VALUES ('2117', '511400000000', '511423000000', '洪雅县');
INSERT INTO `j_position_county` VALUES ('2118', '511400000000', '511424000000', '丹棱县');
INSERT INTO `j_position_county` VALUES ('2119', '511400000000', '511425000000', '青神县');
INSERT INTO `j_position_county` VALUES ('2120', '511500000000', '511502000000', '翠屏区');
INSERT INTO `j_position_county` VALUES ('2121', '511500000000', '511503000000', '南溪区');
INSERT INTO `j_position_county` VALUES ('2122', '511500000000', '511521000000', '宜宾县');
INSERT INTO `j_position_county` VALUES ('2123', '511500000000', '511523000000', '江安县');
INSERT INTO `j_position_county` VALUES ('2124', '511500000000', '511524000000', '长宁县');
INSERT INTO `j_position_county` VALUES ('2125', '511500000000', '511525000000', '高县');
INSERT INTO `j_position_county` VALUES ('2126', '511500000000', '511526000000', '珙县');
INSERT INTO `j_position_county` VALUES ('2127', '511500000000', '511527000000', '筠连县');
INSERT INTO `j_position_county` VALUES ('2128', '511500000000', '511528000000', '兴文县');
INSERT INTO `j_position_county` VALUES ('2129', '511500000000', '511529000000', '屏山县');
INSERT INTO `j_position_county` VALUES ('2130', '511600000000', '511602000000', '广安区');
INSERT INTO `j_position_county` VALUES ('2131', '511600000000', '511603000000', '前锋区');
INSERT INTO `j_position_county` VALUES ('2132', '511600000000', '511621000000', '岳池县');
INSERT INTO `j_position_county` VALUES ('2133', '511600000000', '511622000000', '武胜县');
INSERT INTO `j_position_county` VALUES ('2134', '511600000000', '511623000000', '邻水县');
INSERT INTO `j_position_county` VALUES ('2135', '511600000000', '511681000000', '华蓥市');
INSERT INTO `j_position_county` VALUES ('2136', '511700000000', '511702000000', '通川区');
INSERT INTO `j_position_county` VALUES ('2137', '511700000000', '511703000000', '达川区');
INSERT INTO `j_position_county` VALUES ('2138', '511700000000', '511722000000', '宣汉县');
INSERT INTO `j_position_county` VALUES ('2139', '511700000000', '511723000000', '开江县');
INSERT INTO `j_position_county` VALUES ('2140', '511700000000', '511724000000', '大竹县');
INSERT INTO `j_position_county` VALUES ('2141', '511700000000', '511725000000', '渠县');
INSERT INTO `j_position_county` VALUES ('2142', '511700000000', '511781000000', '万源市');
INSERT INTO `j_position_county` VALUES ('2143', '511800000000', '511802000000', '雨城区');
INSERT INTO `j_position_county` VALUES ('2144', '511800000000', '511803000000', '名山区');
INSERT INTO `j_position_county` VALUES ('2145', '511800000000', '511822000000', '荥经县');
INSERT INTO `j_position_county` VALUES ('2146', '511800000000', '511823000000', '汉源县');
INSERT INTO `j_position_county` VALUES ('2147', '511800000000', '511824000000', '石棉县');
INSERT INTO `j_position_county` VALUES ('2148', '511800000000', '511825000000', '天全县');
INSERT INTO `j_position_county` VALUES ('2149', '511800000000', '511826000000', '芦山县');
INSERT INTO `j_position_county` VALUES ('2150', '511800000000', '511827000000', '宝兴县');
INSERT INTO `j_position_county` VALUES ('2151', '511900000000', '511902000000', '巴州区');
INSERT INTO `j_position_county` VALUES ('2152', '511900000000', '511903000000', '恩阳区');
INSERT INTO `j_position_county` VALUES ('2153', '511900000000', '511921000000', '通江县');
INSERT INTO `j_position_county` VALUES ('2154', '511900000000', '511922000000', '南江县');
INSERT INTO `j_position_county` VALUES ('2155', '511900000000', '511923000000', '平昌县');
INSERT INTO `j_position_county` VALUES ('2156', '512000000000', '512002000000', '雁江区');
INSERT INTO `j_position_county` VALUES ('2157', '512000000000', '512021000000', '安岳县');
INSERT INTO `j_position_county` VALUES ('2158', '512000000000', '512022000000', '乐至县');
INSERT INTO `j_position_county` VALUES ('2159', '512000000000', '512081000000', '简阳市');
INSERT INTO `j_position_county` VALUES ('2160', '513200000000', '513221000000', '汶川县');
INSERT INTO `j_position_county` VALUES ('2161', '513200000000', '513222000000', '理县');
INSERT INTO `j_position_county` VALUES ('2162', '513200000000', '513223000000', '茂县');
INSERT INTO `j_position_county` VALUES ('2163', '513200000000', '513224000000', '松潘县');
INSERT INTO `j_position_county` VALUES ('2164', '513200000000', '513225000000', '九寨沟县');
INSERT INTO `j_position_county` VALUES ('2165', '513200000000', '513226000000', '金川县');
INSERT INTO `j_position_county` VALUES ('2166', '513200000000', '513227000000', '小金县');
INSERT INTO `j_position_county` VALUES ('2167', '513200000000', '513228000000', '黑水县');
INSERT INTO `j_position_county` VALUES ('2168', '513200000000', '513229000000', '马尔康县');
INSERT INTO `j_position_county` VALUES ('2169', '513200000000', '513230000000', '壤塘县');
INSERT INTO `j_position_county` VALUES ('2170', '513200000000', '513231000000', '阿坝县');
INSERT INTO `j_position_county` VALUES ('2171', '513200000000', '513232000000', '若尔盖县');
INSERT INTO `j_position_county` VALUES ('2172', '513200000000', '513233000000', '红原县');
INSERT INTO `j_position_county` VALUES ('2173', '513300000000', '513321000000', '康定县');
INSERT INTO `j_position_county` VALUES ('2174', '513300000000', '513322000000', '泸定县');
INSERT INTO `j_position_county` VALUES ('2175', '513300000000', '513323000000', '丹巴县');
INSERT INTO `j_position_county` VALUES ('2176', '513300000000', '513324000000', '九龙县');
INSERT INTO `j_position_county` VALUES ('2177', '513300000000', '513325000000', '雅江县');
INSERT INTO `j_position_county` VALUES ('2178', '513300000000', '513326000000', '道孚县');
INSERT INTO `j_position_county` VALUES ('2179', '513300000000', '513327000000', '炉霍县');
INSERT INTO `j_position_county` VALUES ('2180', '513300000000', '513328000000', '甘孜县');
INSERT INTO `j_position_county` VALUES ('2181', '513300000000', '513329000000', '新龙县');
INSERT INTO `j_position_county` VALUES ('2182', '513300000000', '513330000000', '德格县');
INSERT INTO `j_position_county` VALUES ('2183', '513300000000', '513331000000', '白玉县');
INSERT INTO `j_position_county` VALUES ('2184', '513300000000', '513332000000', '石渠县');
INSERT INTO `j_position_county` VALUES ('2185', '513300000000', '513333000000', '色达县');
INSERT INTO `j_position_county` VALUES ('2186', '513300000000', '513334000000', '理塘县');
INSERT INTO `j_position_county` VALUES ('2187', '513300000000', '513335000000', '巴塘县');
INSERT INTO `j_position_county` VALUES ('2188', '513300000000', '513336000000', '乡城县');
INSERT INTO `j_position_county` VALUES ('2189', '513300000000', '513337000000', '稻城县');
INSERT INTO `j_position_county` VALUES ('2190', '513300000000', '513338000000', '得荣县');
INSERT INTO `j_position_county` VALUES ('2191', '513400000000', '513401000000', '西昌市');
INSERT INTO `j_position_county` VALUES ('2192', '513400000000', '513422000000', '木里藏族自治县');
INSERT INTO `j_position_county` VALUES ('2193', '513400000000', '513423000000', '盐源县');
INSERT INTO `j_position_county` VALUES ('2194', '513400000000', '513424000000', '德昌县');
INSERT INTO `j_position_county` VALUES ('2195', '513400000000', '513425000000', '会理县');
INSERT INTO `j_position_county` VALUES ('2196', '513400000000', '513426000000', '会东县');
INSERT INTO `j_position_county` VALUES ('2197', '513400000000', '513427000000', '宁南县');
INSERT INTO `j_position_county` VALUES ('2198', '513400000000', '513428000000', '普格县');
INSERT INTO `j_position_county` VALUES ('2199', '513400000000', '513429000000', '布拖县');
INSERT INTO `j_position_county` VALUES ('2200', '513400000000', '513430000000', '金阳县');
INSERT INTO `j_position_county` VALUES ('2201', '513400000000', '513431000000', '昭觉县');
INSERT INTO `j_position_county` VALUES ('2202', '513400000000', '513432000000', '喜德县');
INSERT INTO `j_position_county` VALUES ('2203', '513400000000', '513433000000', '冕宁县');
INSERT INTO `j_position_county` VALUES ('2204', '513400000000', '513434000000', '越西县');
INSERT INTO `j_position_county` VALUES ('2205', '513400000000', '513435000000', '甘洛县');
INSERT INTO `j_position_county` VALUES ('2206', '513400000000', '513436000000', '美姑县');
INSERT INTO `j_position_county` VALUES ('2207', '513400000000', '513437000000', '雷波县');
INSERT INTO `j_position_county` VALUES ('2208', '520100000000', '520102000000', '南明区');
INSERT INTO `j_position_county` VALUES ('2209', '520100000000', '520103000000', '云岩区');
INSERT INTO `j_position_county` VALUES ('2210', '520100000000', '520111000000', '花溪区');
INSERT INTO `j_position_county` VALUES ('2211', '520100000000', '520112000000', '乌当区');
INSERT INTO `j_position_county` VALUES ('2212', '520100000000', '520113000000', '白云区');
INSERT INTO `j_position_county` VALUES ('2213', '520100000000', '520115000000', '观山湖区');
INSERT INTO `j_position_county` VALUES ('2214', '520100000000', '520121000000', '开阳县');
INSERT INTO `j_position_county` VALUES ('2215', '520100000000', '520122000000', '息烽县');
INSERT INTO `j_position_county` VALUES ('2216', '520100000000', '520123000000', '修文县');
INSERT INTO `j_position_county` VALUES ('2217', '520100000000', '520181000000', '清镇市');
INSERT INTO `j_position_county` VALUES ('2218', '520200000000', '520201000000', '钟山区');
INSERT INTO `j_position_county` VALUES ('2219', '520200000000', '520203000000', '六枝特区');
INSERT INTO `j_position_county` VALUES ('2220', '520200000000', '520221000000', '水城县');
INSERT INTO `j_position_county` VALUES ('2221', '520200000000', '520222000000', '盘县');
INSERT INTO `j_position_county` VALUES ('2222', '520300000000', '520302000000', '红花岗区');
INSERT INTO `j_position_county` VALUES ('2223', '520300000000', '520303000000', '汇川区');
INSERT INTO `j_position_county` VALUES ('2224', '520300000000', '520321000000', '遵义县');
INSERT INTO `j_position_county` VALUES ('2225', '520300000000', '520322000000', '桐梓县');
INSERT INTO `j_position_county` VALUES ('2226', '520300000000', '520323000000', '绥阳县');
INSERT INTO `j_position_county` VALUES ('2227', '520300000000', '520324000000', '正安县');
INSERT INTO `j_position_county` VALUES ('2228', '520300000000', '520325000000', '道真仡佬族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2229', '520300000000', '520326000000', '务川仡佬族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2230', '520300000000', '520327000000', '凤冈县');
INSERT INTO `j_position_county` VALUES ('2231', '520300000000', '520328000000', '湄潭县');
INSERT INTO `j_position_county` VALUES ('2232', '520300000000', '520329000000', '余庆县');
INSERT INTO `j_position_county` VALUES ('2233', '520300000000', '520330000000', '习水县');
INSERT INTO `j_position_county` VALUES ('2234', '520300000000', '520381000000', '赤水市');
INSERT INTO `j_position_county` VALUES ('2235', '520300000000', '520382000000', '仁怀市');
INSERT INTO `j_position_county` VALUES ('2236', '520400000000', '520402000000', '西秀区');
INSERT INTO `j_position_county` VALUES ('2237', '520400000000', '520421000000', '平坝县');
INSERT INTO `j_position_county` VALUES ('2238', '520400000000', '520422000000', '普定县');
INSERT INTO `j_position_county` VALUES ('2239', '520400000000', '520423000000', '镇宁布依族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2240', '520400000000', '520424000000', '关岭布依族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2241', '520400000000', '520425000000', '紫云苗族布依族自治县');
INSERT INTO `j_position_county` VALUES ('2242', '520500000000', '520502000000', '七星关区');
INSERT INTO `j_position_county` VALUES ('2243', '520500000000', '520521000000', '大方县');
INSERT INTO `j_position_county` VALUES ('2244', '520500000000', '520522000000', '黔西县');
INSERT INTO `j_position_county` VALUES ('2245', '520500000000', '520523000000', '金沙县');
INSERT INTO `j_position_county` VALUES ('2246', '520500000000', '520524000000', '织金县');
INSERT INTO `j_position_county` VALUES ('2247', '520500000000', '520525000000', '纳雍县');
INSERT INTO `j_position_county` VALUES ('2248', '520500000000', '520526000000', '威宁彝族回族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2249', '520500000000', '520527000000', '赫章县');
INSERT INTO `j_position_county` VALUES ('2250', '520600000000', '520602000000', '碧江区');
INSERT INTO `j_position_county` VALUES ('2251', '520600000000', '520603000000', '万山区');
INSERT INTO `j_position_county` VALUES ('2252', '520600000000', '520621000000', '江口县');
INSERT INTO `j_position_county` VALUES ('2253', '520600000000', '520622000000', '玉屏侗族自治县');
INSERT INTO `j_position_county` VALUES ('2254', '520600000000', '520623000000', '石阡县');
INSERT INTO `j_position_county` VALUES ('2255', '520600000000', '520624000000', '思南县');
INSERT INTO `j_position_county` VALUES ('2256', '520600000000', '520625000000', '印江土家族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2257', '520600000000', '520626000000', '德江县');
INSERT INTO `j_position_county` VALUES ('2258', '520600000000', '520627000000', '沿河土家族自治县');
INSERT INTO `j_position_county` VALUES ('2259', '520600000000', '520628000000', '松桃苗族自治县');
INSERT INTO `j_position_county` VALUES ('2260', '522300000000', '522301000000', '兴义市');
INSERT INTO `j_position_county` VALUES ('2261', '522300000000', '522322000000', '兴仁县');
INSERT INTO `j_position_county` VALUES ('2262', '522300000000', '522323000000', '普安县');
INSERT INTO `j_position_county` VALUES ('2263', '522300000000', '522324000000', '晴隆县');
INSERT INTO `j_position_county` VALUES ('2264', '522300000000', '522325000000', '贞丰县');
INSERT INTO `j_position_county` VALUES ('2265', '522300000000', '522326000000', '望谟县');
INSERT INTO `j_position_county` VALUES ('2266', '522300000000', '522327000000', '册亨县');
INSERT INTO `j_position_county` VALUES ('2267', '522300000000', '522328000000', '安龙县');
INSERT INTO `j_position_county` VALUES ('2268', '522600000000', '522601000000', '凯里市');
INSERT INTO `j_position_county` VALUES ('2269', '522600000000', '522622000000', '黄平县');
INSERT INTO `j_position_county` VALUES ('2270', '522600000000', '522623000000', '施秉县');
INSERT INTO `j_position_county` VALUES ('2271', '522600000000', '522624000000', '三穗县');
INSERT INTO `j_position_county` VALUES ('2272', '522600000000', '522625000000', '镇远县');
INSERT INTO `j_position_county` VALUES ('2273', '522600000000', '522626000000', '岑巩县');
INSERT INTO `j_position_county` VALUES ('2274', '522600000000', '522627000000', '天柱县');
INSERT INTO `j_position_county` VALUES ('2275', '522600000000', '522628000000', '锦屏县');
INSERT INTO `j_position_county` VALUES ('2276', '522600000000', '522629000000', '剑河县');
INSERT INTO `j_position_county` VALUES ('2277', '522600000000', '522630000000', '台江县');
INSERT INTO `j_position_county` VALUES ('2278', '522600000000', '522631000000', '黎平县');
INSERT INTO `j_position_county` VALUES ('2279', '522600000000', '522632000000', '榕江县');
INSERT INTO `j_position_county` VALUES ('2280', '522600000000', '522633000000', '从江县');
INSERT INTO `j_position_county` VALUES ('2281', '522600000000', '522634000000', '雷山县');
INSERT INTO `j_position_county` VALUES ('2282', '522600000000', '522635000000', '麻江县');
INSERT INTO `j_position_county` VALUES ('2283', '522600000000', '522636000000', '丹寨县');
INSERT INTO `j_position_county` VALUES ('2284', '522700000000', '522701000000', '都匀市');
INSERT INTO `j_position_county` VALUES ('2285', '522700000000', '522702000000', '福泉市');
INSERT INTO `j_position_county` VALUES ('2286', '522700000000', '522722000000', '荔波县');
INSERT INTO `j_position_county` VALUES ('2287', '522700000000', '522723000000', '贵定县');
INSERT INTO `j_position_county` VALUES ('2288', '522700000000', '522725000000', '瓮安县');
INSERT INTO `j_position_county` VALUES ('2289', '522700000000', '522726000000', '独山县');
INSERT INTO `j_position_county` VALUES ('2290', '522700000000', '522727000000', '平塘县');
INSERT INTO `j_position_county` VALUES ('2291', '522700000000', '522728000000', '罗甸县');
INSERT INTO `j_position_county` VALUES ('2292', '522700000000', '522729000000', '长顺县');
INSERT INTO `j_position_county` VALUES ('2293', '522700000000', '522730000000', '龙里县');
INSERT INTO `j_position_county` VALUES ('2294', '522700000000', '522731000000', '惠水县');
INSERT INTO `j_position_county` VALUES ('2295', '522700000000', '522732000000', '三都水族自治县');
INSERT INTO `j_position_county` VALUES ('2296', '530100000000', '530102000000', '五华区');
INSERT INTO `j_position_county` VALUES ('2297', '530100000000', '530103000000', '盘龙区');
INSERT INTO `j_position_county` VALUES ('2298', '530100000000', '530111000000', '官渡区');
INSERT INTO `j_position_county` VALUES ('2299', '530100000000', '530112000000', '西山区');
INSERT INTO `j_position_county` VALUES ('2300', '530100000000', '530113000000', '东川区');
INSERT INTO `j_position_county` VALUES ('2301', '530100000000', '530114000000', '呈贡区');
INSERT INTO `j_position_county` VALUES ('2302', '530100000000', '530122000000', '晋宁县');
INSERT INTO `j_position_county` VALUES ('2303', '530100000000', '530124000000', '富民县');
INSERT INTO `j_position_county` VALUES ('2304', '530100000000', '530125000000', '宜良县');
INSERT INTO `j_position_county` VALUES ('2305', '530100000000', '530126000000', '石林彝族自治县');
INSERT INTO `j_position_county` VALUES ('2306', '530100000000', '530127000000', '嵩明县');
INSERT INTO `j_position_county` VALUES ('2307', '530100000000', '530128000000', '禄劝彝族苗族自治县');
INSERT INTO `j_position_county` VALUES ('2308', '530100000000', '530129000000', '寻甸回族彝族自治县');
INSERT INTO `j_position_county` VALUES ('2309', '530100000000', '530181000000', '安宁市');
INSERT INTO `j_position_county` VALUES ('2310', '530300000000', '530302000000', '麒麟区');
INSERT INTO `j_position_county` VALUES ('2311', '530300000000', '530321000000', '马龙县');
INSERT INTO `j_position_county` VALUES ('2312', '530300000000', '530322000000', '陆良县');
INSERT INTO `j_position_county` VALUES ('2313', '530300000000', '530323000000', '师宗县');
INSERT INTO `j_position_county` VALUES ('2314', '530300000000', '530324000000', '罗平县');
INSERT INTO `j_position_county` VALUES ('2315', '530300000000', '530325000000', '富源县');
INSERT INTO `j_position_county` VALUES ('2316', '530300000000', '530326000000', '会泽县');
INSERT INTO `j_position_county` VALUES ('2317', '530300000000', '530328000000', '沾益县');
INSERT INTO `j_position_county` VALUES ('2318', '530300000000', '530381000000', '宣威市');
INSERT INTO `j_position_county` VALUES ('2319', '530400000000', '530402000000', '红塔区');
INSERT INTO `j_position_county` VALUES ('2320', '530400000000', '530421000000', '江川县');
INSERT INTO `j_position_county` VALUES ('2321', '530400000000', '530422000000', '澄江县');
INSERT INTO `j_position_county` VALUES ('2322', '530400000000', '530423000000', '通海县');
INSERT INTO `j_position_county` VALUES ('2323', '530400000000', '530424000000', '华宁县');
INSERT INTO `j_position_county` VALUES ('2324', '530400000000', '530425000000', '易门县');
INSERT INTO `j_position_county` VALUES ('2325', '530400000000', '530426000000', '峨山彝族自治县');
INSERT INTO `j_position_county` VALUES ('2326', '530400000000', '530427000000', '新平彝族傣族自治县');
INSERT INTO `j_position_county` VALUES ('2327', '530400000000', '530428000000', '元江哈尼族彝族傣族自治县');
INSERT INTO `j_position_county` VALUES ('2328', '530500000000', '530502000000', '隆阳区');
INSERT INTO `j_position_county` VALUES ('2329', '530500000000', '530521000000', '施甸县');
INSERT INTO `j_position_county` VALUES ('2330', '530500000000', '530522000000', '腾冲县');
INSERT INTO `j_position_county` VALUES ('2331', '530500000000', '530523000000', '龙陵县');
INSERT INTO `j_position_county` VALUES ('2332', '530500000000', '530524000000', '昌宁县');
INSERT INTO `j_position_county` VALUES ('2333', '530600000000', '530602000000', '昭阳区');
INSERT INTO `j_position_county` VALUES ('2334', '530600000000', '530621000000', '鲁甸县');
INSERT INTO `j_position_county` VALUES ('2335', '530600000000', '530622000000', '巧家县');
INSERT INTO `j_position_county` VALUES ('2336', '530600000000', '530623000000', '盐津县');
INSERT INTO `j_position_county` VALUES ('2337', '530600000000', '530624000000', '大关县');
INSERT INTO `j_position_county` VALUES ('2338', '530600000000', '530625000000', '永善县');
INSERT INTO `j_position_county` VALUES ('2339', '530600000000', '530626000000', '绥江县');
INSERT INTO `j_position_county` VALUES ('2340', '530600000000', '530627000000', '镇雄县');
INSERT INTO `j_position_county` VALUES ('2341', '530600000000', '530628000000', '彝良县');
INSERT INTO `j_position_county` VALUES ('2342', '530600000000', '530629000000', '威信县');
INSERT INTO `j_position_county` VALUES ('2343', '530600000000', '530630000000', '水富县');
INSERT INTO `j_position_county` VALUES ('2344', '530700000000', '530702000000', '古城区');
INSERT INTO `j_position_county` VALUES ('2345', '530700000000', '530721000000', '玉龙纳西族自治县');
INSERT INTO `j_position_county` VALUES ('2346', '530700000000', '530722000000', '永胜县');
INSERT INTO `j_position_county` VALUES ('2347', '530700000000', '530723000000', '华坪县');
INSERT INTO `j_position_county` VALUES ('2348', '530700000000', '530724000000', '宁蒗彝族自治县');
INSERT INTO `j_position_county` VALUES ('2349', '530800000000', '530802000000', '思茅区');
INSERT INTO `j_position_county` VALUES ('2350', '530800000000', '530821000000', '宁洱哈尼族彝族自治县');
INSERT INTO `j_position_county` VALUES ('2351', '530800000000', '530822000000', '墨江哈尼族自治县');
INSERT INTO `j_position_county` VALUES ('2352', '530800000000', '530823000000', '景东彝族自治县');
INSERT INTO `j_position_county` VALUES ('2353', '530800000000', '530824000000', '景谷傣族彝族自治县');
INSERT INTO `j_position_county` VALUES ('2354', '530800000000', '530825000000', '镇沅彝族哈尼族拉祜族自治县');
INSERT INTO `j_position_county` VALUES ('2355', '530800000000', '530826000000', '江城哈尼族彝族自治县');
INSERT INTO `j_position_county` VALUES ('2356', '530800000000', '530827000000', '孟连傣族拉祜族佤族自治县');
INSERT INTO `j_position_county` VALUES ('2357', '530800000000', '530828000000', '澜沧拉祜族自治县');
INSERT INTO `j_position_county` VALUES ('2358', '530800000000', '530829000000', '西盟佤族自治县');
INSERT INTO `j_position_county` VALUES ('2359', '530900000000', '530902000000', '临翔区');
INSERT INTO `j_position_county` VALUES ('2360', '530900000000', '530921000000', '凤庆县');
INSERT INTO `j_position_county` VALUES ('2361', '530900000000', '530922000000', '云县');
INSERT INTO `j_position_county` VALUES ('2362', '530900000000', '530923000000', '永德县');
INSERT INTO `j_position_county` VALUES ('2363', '530900000000', '530924000000', '镇康县');
INSERT INTO `j_position_county` VALUES ('2364', '530900000000', '530925000000', '双江拉祜族佤族布朗族傣族自治县');
INSERT INTO `j_position_county` VALUES ('2365', '530900000000', '530926000000', '耿马傣族佤族自治县');
INSERT INTO `j_position_county` VALUES ('2366', '530900000000', '530927000000', '沧源佤族自治县');
INSERT INTO `j_position_county` VALUES ('2367', '532300000000', '532301000000', '楚雄市');
INSERT INTO `j_position_county` VALUES ('2368', '532300000000', '532322000000', '双柏县');
INSERT INTO `j_position_county` VALUES ('2369', '532300000000', '532323000000', '牟定县');
INSERT INTO `j_position_county` VALUES ('2370', '532300000000', '532324000000', '南华县');
INSERT INTO `j_position_county` VALUES ('2371', '532300000000', '532325000000', '姚安县');
INSERT INTO `j_position_county` VALUES ('2372', '532300000000', '532326000000', '大姚县');
INSERT INTO `j_position_county` VALUES ('2373', '532300000000', '532327000000', '永仁县');
INSERT INTO `j_position_county` VALUES ('2374', '532300000000', '532328000000', '元谋县');
INSERT INTO `j_position_county` VALUES ('2375', '532300000000', '532329000000', '武定县');
INSERT INTO `j_position_county` VALUES ('2376', '532300000000', '532331000000', '禄丰县');
INSERT INTO `j_position_county` VALUES ('2377', '532500000000', '532501000000', '个旧市');
INSERT INTO `j_position_county` VALUES ('2378', '532500000000', '532502000000', '开远市');
INSERT INTO `j_position_county` VALUES ('2379', '532500000000', '532503000000', '蒙自市');
INSERT INTO `j_position_county` VALUES ('2380', '532500000000', '532504000000', '弥勒市');
INSERT INTO `j_position_county` VALUES ('2381', '532500000000', '532523000000', '屏边苗族自治县');
INSERT INTO `j_position_county` VALUES ('2382', '532500000000', '532524000000', '建水县');
INSERT INTO `j_position_county` VALUES ('2383', '532500000000', '532525000000', '石屏县');
INSERT INTO `j_position_county` VALUES ('2384', '532500000000', '532527000000', '泸西县');
INSERT INTO `j_position_county` VALUES ('2385', '532500000000', '532528000000', '元阳县');
INSERT INTO `j_position_county` VALUES ('2386', '532500000000', '532529000000', '红河县');
INSERT INTO `j_position_county` VALUES ('2387', '532500000000', '532530000000', '金平苗族瑶族傣族自治县');
INSERT INTO `j_position_county` VALUES ('2388', '532500000000', '532531000000', '绿春县');
INSERT INTO `j_position_county` VALUES ('2389', '532500000000', '532532000000', '河口瑶族自治县');
INSERT INTO `j_position_county` VALUES ('2390', '532600000000', '532601000000', '文山市');
INSERT INTO `j_position_county` VALUES ('2391', '532600000000', '532622000000', '砚山县');
INSERT INTO `j_position_county` VALUES ('2392', '532600000000', '532623000000', '西畴县');
INSERT INTO `j_position_county` VALUES ('2393', '532600000000', '532624000000', '麻栗坡县');
INSERT INTO `j_position_county` VALUES ('2394', '532600000000', '532625000000', '马关县');
INSERT INTO `j_position_county` VALUES ('2395', '532600000000', '532626000000', '丘北县');
INSERT INTO `j_position_county` VALUES ('2396', '532600000000', '532627000000', '广南县');
INSERT INTO `j_position_county` VALUES ('2397', '532600000000', '532628000000', '富宁县');
INSERT INTO `j_position_county` VALUES ('2398', '532800000000', '532801000000', '景洪市');
INSERT INTO `j_position_county` VALUES ('2399', '532800000000', '532822000000', '勐海县');
INSERT INTO `j_position_county` VALUES ('2400', '532800000000', '532823000000', '勐腊县');
INSERT INTO `j_position_county` VALUES ('2401', '532900000000', '532901000000', '大理市');
INSERT INTO `j_position_county` VALUES ('2402', '532900000000', '532922000000', '漾濞彝族自治县');
INSERT INTO `j_position_county` VALUES ('2403', '532900000000', '532923000000', '祥云县');
INSERT INTO `j_position_county` VALUES ('2404', '532900000000', '532924000000', '宾川县');
INSERT INTO `j_position_county` VALUES ('2405', '532900000000', '532925000000', '弥渡县');
INSERT INTO `j_position_county` VALUES ('2406', '532900000000', '532926000000', '南涧彝族自治县');
INSERT INTO `j_position_county` VALUES ('2407', '532900000000', '532927000000', '巍山彝族回族自治县');
INSERT INTO `j_position_county` VALUES ('2408', '532900000000', '532928000000', '永平县');
INSERT INTO `j_position_county` VALUES ('2409', '532900000000', '532929000000', '云龙县');
INSERT INTO `j_position_county` VALUES ('2410', '532900000000', '532930000000', '洱源县');
INSERT INTO `j_position_county` VALUES ('2411', '532900000000', '532931000000', '剑川县');
INSERT INTO `j_position_county` VALUES ('2412', '532900000000', '532932000000', '鹤庆县');
INSERT INTO `j_position_county` VALUES ('2413', '533100000000', '533102000000', '瑞丽市');
INSERT INTO `j_position_county` VALUES ('2414', '533100000000', '533103000000', '芒市');
INSERT INTO `j_position_county` VALUES ('2415', '533100000000', '533122000000', '梁河县');
INSERT INTO `j_position_county` VALUES ('2416', '533100000000', '533123000000', '盈江县');
INSERT INTO `j_position_county` VALUES ('2417', '533100000000', '533124000000', '陇川县');
INSERT INTO `j_position_county` VALUES ('2418', '533300000000', '533321000000', '泸水县');
INSERT INTO `j_position_county` VALUES ('2419', '533300000000', '533323000000', '福贡县');
INSERT INTO `j_position_county` VALUES ('2420', '533300000000', '533324000000', '贡山独龙族怒族自治县');
INSERT INTO `j_position_county` VALUES ('2421', '533300000000', '533325000000', '兰坪白族普米族自治县');
INSERT INTO `j_position_county` VALUES ('2422', '533400000000', '533421000000', '香格里拉县');
INSERT INTO `j_position_county` VALUES ('2423', '533400000000', '533422000000', '德钦县');
INSERT INTO `j_position_county` VALUES ('2424', '533400000000', '533423000000', '维西傈僳族自治县');
INSERT INTO `j_position_county` VALUES ('2425', '540100000000', '540102000000', '城关区');
INSERT INTO `j_position_county` VALUES ('2426', '540100000000', '540121000000', '林周县');
INSERT INTO `j_position_county` VALUES ('2427', '540100000000', '540122000000', '当雄县');
INSERT INTO `j_position_county` VALUES ('2428', '540100000000', '540123000000', '尼木县');
INSERT INTO `j_position_county` VALUES ('2429', '540100000000', '540124000000', '曲水县');
INSERT INTO `j_position_county` VALUES ('2430', '540100000000', '540125000000', '堆龙德庆县');
INSERT INTO `j_position_county` VALUES ('2431', '540100000000', '540126000000', '达孜县');
INSERT INTO `j_position_county` VALUES ('2432', '540100000000', '540127000000', '墨竹工卡县');
INSERT INTO `j_position_county` VALUES ('2433', '542100000000', '542121000000', '昌都县');
INSERT INTO `j_position_county` VALUES ('2434', '542100000000', '542122000000', '江达县');
INSERT INTO `j_position_county` VALUES ('2435', '542100000000', '542123000000', '贡觉县');
INSERT INTO `j_position_county` VALUES ('2436', '542100000000', '542124000000', '类乌齐县');
INSERT INTO `j_position_county` VALUES ('2437', '542100000000', '542125000000', '丁青县');
INSERT INTO `j_position_county` VALUES ('2438', '542100000000', '542126000000', '察雅县');
INSERT INTO `j_position_county` VALUES ('2439', '542100000000', '542127000000', '八宿县');
INSERT INTO `j_position_county` VALUES ('2440', '542100000000', '542128000000', '左贡县');
INSERT INTO `j_position_county` VALUES ('2441', '542100000000', '542129000000', '芒康县');
INSERT INTO `j_position_county` VALUES ('2442', '542100000000', '542132000000', '洛隆县');
INSERT INTO `j_position_county` VALUES ('2443', '542100000000', '542133000000', '边坝县');
INSERT INTO `j_position_county` VALUES ('2444', '542200000000', '542221000000', '乃东县');
INSERT INTO `j_position_county` VALUES ('2445', '542200000000', '542222000000', '扎囊县');
INSERT INTO `j_position_county` VALUES ('2446', '542200000000', '542223000000', '贡嘎县');
INSERT INTO `j_position_county` VALUES ('2447', '542200000000', '542224000000', '桑日县');
INSERT INTO `j_position_county` VALUES ('2448', '542200000000', '542225000000', '琼结县');
INSERT INTO `j_position_county` VALUES ('2449', '542200000000', '542226000000', '曲松县');
INSERT INTO `j_position_county` VALUES ('2450', '542200000000', '542227000000', '措美县');
INSERT INTO `j_position_county` VALUES ('2451', '542200000000', '542228000000', '洛扎县');
INSERT INTO `j_position_county` VALUES ('2452', '542200000000', '542229000000', '加查县');
INSERT INTO `j_position_county` VALUES ('2453', '542200000000', '542231000000', '隆子县');
INSERT INTO `j_position_county` VALUES ('2454', '542200000000', '542232000000', '错那县');
INSERT INTO `j_position_county` VALUES ('2455', '542200000000', '542233000000', '浪卡子县');
INSERT INTO `j_position_county` VALUES ('2456', '542300000000', '542301000000', '日喀则市');
INSERT INTO `j_position_county` VALUES ('2457', '542300000000', '542322000000', '南木林县');
INSERT INTO `j_position_county` VALUES ('2458', '542300000000', '542323000000', '江孜县');
INSERT INTO `j_position_county` VALUES ('2459', '542300000000', '542324000000', '定日县');
INSERT INTO `j_position_county` VALUES ('2460', '542300000000', '542325000000', '萨迦县');
INSERT INTO `j_position_county` VALUES ('2461', '542300000000', '542326000000', '拉孜县');
INSERT INTO `j_position_county` VALUES ('2462', '542300000000', '542327000000', '昂仁县');
INSERT INTO `j_position_county` VALUES ('2463', '542300000000', '542328000000', '谢通门县');
INSERT INTO `j_position_county` VALUES ('2464', '542300000000', '542329000000', '白朗县');
INSERT INTO `j_position_county` VALUES ('2465', '542300000000', '542330000000', '仁布县');
INSERT INTO `j_position_county` VALUES ('2466', '542300000000', '542331000000', '康马县');
INSERT INTO `j_position_county` VALUES ('2467', '542300000000', '542332000000', '定结县');
INSERT INTO `j_position_county` VALUES ('2468', '542300000000', '542333000000', '仲巴县');
INSERT INTO `j_position_county` VALUES ('2469', '542300000000', '542334000000', '亚东县');
INSERT INTO `j_position_county` VALUES ('2470', '542300000000', '542335000000', '吉隆县');
INSERT INTO `j_position_county` VALUES ('2471', '542300000000', '542336000000', '聂拉木县');
INSERT INTO `j_position_county` VALUES ('2472', '542300000000', '542337000000', '萨嘎县');
INSERT INTO `j_position_county` VALUES ('2473', '542300000000', '542338000000', '岗巴县');
INSERT INTO `j_position_county` VALUES ('2474', '542400000000', '542421000000', '那曲县');
INSERT INTO `j_position_county` VALUES ('2475', '542400000000', '542422000000', '嘉黎县');
INSERT INTO `j_position_county` VALUES ('2476', '542400000000', '542423000000', '比如县');
INSERT INTO `j_position_county` VALUES ('2477', '542400000000', '542424000000', '聂荣县');
INSERT INTO `j_position_county` VALUES ('2478', '542400000000', '542425000000', '安多县');
INSERT INTO `j_position_county` VALUES ('2479', '542400000000', '542426000000', '申扎县');
INSERT INTO `j_position_county` VALUES ('2480', '542400000000', '542427000000', '索县');
INSERT INTO `j_position_county` VALUES ('2481', '542400000000', '542428000000', '班戈县');
INSERT INTO `j_position_county` VALUES ('2482', '542400000000', '542429000000', '巴青县');
INSERT INTO `j_position_county` VALUES ('2483', '542400000000', '542430000000', '尼玛县');
INSERT INTO `j_position_county` VALUES ('2484', '542400000000', '542431000000', '双湖县');
INSERT INTO `j_position_county` VALUES ('2485', '542500000000', '542521000000', '普兰县');
INSERT INTO `j_position_county` VALUES ('2486', '542500000000', '542522000000', '札达县');
INSERT INTO `j_position_county` VALUES ('2487', '542500000000', '542523000000', '噶尔县');
INSERT INTO `j_position_county` VALUES ('2488', '542500000000', '542524000000', '日土县');
INSERT INTO `j_position_county` VALUES ('2489', '542500000000', '542525000000', '革吉县');
INSERT INTO `j_position_county` VALUES ('2490', '542500000000', '542526000000', '改则县');
INSERT INTO `j_position_county` VALUES ('2491', '542500000000', '542527000000', '措勤县');
INSERT INTO `j_position_county` VALUES ('2492', '542600000000', '542621000000', '林芝县');
INSERT INTO `j_position_county` VALUES ('2493', '542600000000', '542622000000', '工布江达县');
INSERT INTO `j_position_county` VALUES ('2494', '542600000000', '542623000000', '米林县');
INSERT INTO `j_position_county` VALUES ('2495', '542600000000', '542624000000', '墨脱县');
INSERT INTO `j_position_county` VALUES ('2496', '542600000000', '542625000000', '波密县');
INSERT INTO `j_position_county` VALUES ('2497', '542600000000', '542626000000', '察隅县');
INSERT INTO `j_position_county` VALUES ('2498', '542600000000', '542627000000', '朗县');
INSERT INTO `j_position_county` VALUES ('2499', '610100000000', '610102000000', '新城区');
INSERT INTO `j_position_county` VALUES ('2500', '610100000000', '610103000000', '碑林区');
INSERT INTO `j_position_county` VALUES ('2501', '610100000000', '610104000000', '莲湖区');
INSERT INTO `j_position_county` VALUES ('2502', '610100000000', '610111000000', '灞桥区');
INSERT INTO `j_position_county` VALUES ('2503', '610100000000', '610112000000', '未央区');
INSERT INTO `j_position_county` VALUES ('2504', '610100000000', '610113000000', '雁塔区');
INSERT INTO `j_position_county` VALUES ('2505', '610100000000', '610114000000', '阎良区');
INSERT INTO `j_position_county` VALUES ('2506', '610100000000', '610115000000', '临潼区');
INSERT INTO `j_position_county` VALUES ('2507', '610100000000', '610116000000', '长安区');
INSERT INTO `j_position_county` VALUES ('2508', '610100000000', '610122000000', '蓝田县');
INSERT INTO `j_position_county` VALUES ('2509', '610100000000', '610124000000', '周至县');
INSERT INTO `j_position_county` VALUES ('2510', '610100000000', '610125000000', '户县');
INSERT INTO `j_position_county` VALUES ('2511', '610100000000', '610126000000', '高陵县');
INSERT INTO `j_position_county` VALUES ('2512', '610200000000', '610202000000', '王益区');
INSERT INTO `j_position_county` VALUES ('2513', '610200000000', '610203000000', '印台区');
INSERT INTO `j_position_county` VALUES ('2514', '610200000000', '610204000000', '耀州区');
INSERT INTO `j_position_county` VALUES ('2515', '610200000000', '610222000000', '宜君县');
INSERT INTO `j_position_county` VALUES ('2516', '610300000000', '610302000000', '渭滨区');
INSERT INTO `j_position_county` VALUES ('2517', '610300000000', '610303000000', '金台区');
INSERT INTO `j_position_county` VALUES ('2518', '610300000000', '610304000000', '陈仓区');
INSERT INTO `j_position_county` VALUES ('2519', '610300000000', '610322000000', '凤翔县');
INSERT INTO `j_position_county` VALUES ('2520', '610300000000', '610323000000', '岐山县');
INSERT INTO `j_position_county` VALUES ('2521', '610300000000', '610324000000', '扶风县');
INSERT INTO `j_position_county` VALUES ('2522', '610300000000', '610326000000', '眉县');
INSERT INTO `j_position_county` VALUES ('2523', '610300000000', '610327000000', '陇县');
INSERT INTO `j_position_county` VALUES ('2524', '610300000000', '610328000000', '千阳县');
INSERT INTO `j_position_county` VALUES ('2525', '610300000000', '610329000000', '麟游县');
INSERT INTO `j_position_county` VALUES ('2526', '610300000000', '610330000000', '凤县');
INSERT INTO `j_position_county` VALUES ('2527', '610300000000', '610331000000', '太白县');
INSERT INTO `j_position_county` VALUES ('2528', '610400000000', '610402000000', '秦都区');
INSERT INTO `j_position_county` VALUES ('2529', '610400000000', '610403000000', '杨陵区');
INSERT INTO `j_position_county` VALUES ('2530', '610400000000', '610404000000', '渭城区');
INSERT INTO `j_position_county` VALUES ('2531', '610400000000', '610422000000', '三原县');
INSERT INTO `j_position_county` VALUES ('2532', '610400000000', '610423000000', '泾阳县');
INSERT INTO `j_position_county` VALUES ('2533', '610400000000', '610424000000', '乾县');
INSERT INTO `j_position_county` VALUES ('2534', '610400000000', '610425000000', '礼泉县');
INSERT INTO `j_position_county` VALUES ('2535', '610400000000', '610426000000', '永寿县');
INSERT INTO `j_position_county` VALUES ('2536', '610400000000', '610427000000', '彬县');
INSERT INTO `j_position_county` VALUES ('2537', '610400000000', '610428000000', '长武县');
INSERT INTO `j_position_county` VALUES ('2538', '610400000000', '610429000000', '旬邑县');
INSERT INTO `j_position_county` VALUES ('2539', '610400000000', '610430000000', '淳化县');
INSERT INTO `j_position_county` VALUES ('2540', '610400000000', '610431000000', '武功县');
INSERT INTO `j_position_county` VALUES ('2541', '610400000000', '610481000000', '兴平市');
INSERT INTO `j_position_county` VALUES ('2542', '610500000000', '610502000000', '临渭区');
INSERT INTO `j_position_county` VALUES ('2543', '610500000000', '610521000000', '华县');
INSERT INTO `j_position_county` VALUES ('2544', '610500000000', '610522000000', '潼关县');
INSERT INTO `j_position_county` VALUES ('2545', '610500000000', '610523000000', '大荔县');
INSERT INTO `j_position_county` VALUES ('2546', '610500000000', '610524000000', '合阳县');
INSERT INTO `j_position_county` VALUES ('2547', '610500000000', '610525000000', '澄城县');
INSERT INTO `j_position_county` VALUES ('2548', '610500000000', '610526000000', '蒲城县');
INSERT INTO `j_position_county` VALUES ('2549', '610500000000', '610527000000', '白水县');
INSERT INTO `j_position_county` VALUES ('2550', '610500000000', '610528000000', '富平县');
INSERT INTO `j_position_county` VALUES ('2551', '610500000000', '610581000000', '韩城市');
INSERT INTO `j_position_county` VALUES ('2552', '610500000000', '610582000000', '华阴市');
INSERT INTO `j_position_county` VALUES ('2553', '610600000000', '610602000000', '宝塔区');
INSERT INTO `j_position_county` VALUES ('2554', '610600000000', '610621000000', '延长县');
INSERT INTO `j_position_county` VALUES ('2555', '610600000000', '610622000000', '延川县');
INSERT INTO `j_position_county` VALUES ('2556', '610600000000', '610623000000', '子长县');
INSERT INTO `j_position_county` VALUES ('2557', '610600000000', '610624000000', '安塞县');
INSERT INTO `j_position_county` VALUES ('2558', '610600000000', '610625000000', '志丹县');
INSERT INTO `j_position_county` VALUES ('2559', '610600000000', '610626000000', '吴起县');
INSERT INTO `j_position_county` VALUES ('2560', '610600000000', '610627000000', '甘泉县');
INSERT INTO `j_position_county` VALUES ('2561', '610600000000', '610628000000', '富县');
INSERT INTO `j_position_county` VALUES ('2562', '610600000000', '610629000000', '洛川县');
INSERT INTO `j_position_county` VALUES ('2563', '610600000000', '610630000000', '宜川县');
INSERT INTO `j_position_county` VALUES ('2564', '610600000000', '610631000000', '黄龙县');
INSERT INTO `j_position_county` VALUES ('2565', '610600000000', '610632000000', '黄陵县');
INSERT INTO `j_position_county` VALUES ('2566', '610700000000', '610702000000', '汉台区');
INSERT INTO `j_position_county` VALUES ('2567', '610700000000', '610721000000', '南郑县');
INSERT INTO `j_position_county` VALUES ('2568', '610700000000', '610722000000', '城固县');
INSERT INTO `j_position_county` VALUES ('2569', '610700000000', '610723000000', '洋县');
INSERT INTO `j_position_county` VALUES ('2570', '610700000000', '610724000000', '西乡县');
INSERT INTO `j_position_county` VALUES ('2571', '610700000000', '610725000000', '勉县');
INSERT INTO `j_position_county` VALUES ('2572', '610700000000', '610726000000', '宁强县');
INSERT INTO `j_position_county` VALUES ('2573', '610700000000', '610727000000', '略阳县');
INSERT INTO `j_position_county` VALUES ('2574', '610700000000', '610728000000', '镇巴县');
INSERT INTO `j_position_county` VALUES ('2575', '610700000000', '610729000000', '留坝县');
INSERT INTO `j_position_county` VALUES ('2576', '610700000000', '610730000000', '佛坪县');
INSERT INTO `j_position_county` VALUES ('2577', '610800000000', '610802000000', '榆阳区');
INSERT INTO `j_position_county` VALUES ('2578', '610800000000', '610821000000', '神木县');
INSERT INTO `j_position_county` VALUES ('2579', '610800000000', '610822000000', '府谷县');
INSERT INTO `j_position_county` VALUES ('2580', '610800000000', '610823000000', '横山县');
INSERT INTO `j_position_county` VALUES ('2581', '610800000000', '610824000000', '靖边县');
INSERT INTO `j_position_county` VALUES ('2582', '610800000000', '610825000000', '定边县');
INSERT INTO `j_position_county` VALUES ('2583', '610800000000', '610826000000', '绥德县');
INSERT INTO `j_position_county` VALUES ('2584', '610800000000', '610827000000', '米脂县');
INSERT INTO `j_position_county` VALUES ('2585', '610800000000', '610828000000', '佳县');
INSERT INTO `j_position_county` VALUES ('2586', '610800000000', '610829000000', '吴堡县');
INSERT INTO `j_position_county` VALUES ('2587', '610800000000', '610830000000', '清涧县');
INSERT INTO `j_position_county` VALUES ('2588', '610800000000', '610831000000', '子洲县');
INSERT INTO `j_position_county` VALUES ('2589', '610900000000', '610902000000', '汉滨区');
INSERT INTO `j_position_county` VALUES ('2590', '610900000000', '610921000000', '汉阴县');
INSERT INTO `j_position_county` VALUES ('2591', '610900000000', '610922000000', '石泉县');
INSERT INTO `j_position_county` VALUES ('2592', '610900000000', '610923000000', '宁陕县');
INSERT INTO `j_position_county` VALUES ('2593', '610900000000', '610924000000', '紫阳县');
INSERT INTO `j_position_county` VALUES ('2594', '610900000000', '610925000000', '岚皋县');
INSERT INTO `j_position_county` VALUES ('2595', '610900000000', '610926000000', '平利县');
INSERT INTO `j_position_county` VALUES ('2596', '610900000000', '610927000000', '镇坪县');
INSERT INTO `j_position_county` VALUES ('2597', '610900000000', '610928000000', '旬阳县');
INSERT INTO `j_position_county` VALUES ('2598', '610900000000', '610929000000', '白河县');
INSERT INTO `j_position_county` VALUES ('2599', '611000000000', '611002000000', '商州区');
INSERT INTO `j_position_county` VALUES ('2600', '611000000000', '611021000000', '洛南县');
INSERT INTO `j_position_county` VALUES ('2601', '611000000000', '611022000000', '丹凤县');
INSERT INTO `j_position_county` VALUES ('2602', '611000000000', '611023000000', '商南县');
INSERT INTO `j_position_county` VALUES ('2603', '611000000000', '611024000000', '山阳县');
INSERT INTO `j_position_county` VALUES ('2604', '611000000000', '611025000000', '镇安县');
INSERT INTO `j_position_county` VALUES ('2605', '611000000000', '611026000000', '柞水县');
INSERT INTO `j_position_county` VALUES ('2606', '620100000000', '620102000000', '城关区');
INSERT INTO `j_position_county` VALUES ('2607', '620100000000', '620103000000', '七里河区');
INSERT INTO `j_position_county` VALUES ('2608', '620100000000', '620104000000', '西固区');
INSERT INTO `j_position_county` VALUES ('2609', '620100000000', '620105000000', '安宁区');
INSERT INTO `j_position_county` VALUES ('2610', '620100000000', '620111000000', '红古区');
INSERT INTO `j_position_county` VALUES ('2611', '620100000000', '620121000000', '永登县');
INSERT INTO `j_position_county` VALUES ('2612', '620100000000', '620122000000', '皋兰县');
INSERT INTO `j_position_county` VALUES ('2613', '620100000000', '620123000000', '榆中县');
INSERT INTO `j_position_county` VALUES ('2614', '620200000000', '620201000000', '市辖区');
INSERT INTO `j_position_county` VALUES ('2615', '620300000000', '620302000000', '金川区');
INSERT INTO `j_position_county` VALUES ('2616', '620300000000', '620321000000', '永昌县');
INSERT INTO `j_position_county` VALUES ('2617', '620400000000', '620402000000', '白银区');
INSERT INTO `j_position_county` VALUES ('2618', '620400000000', '620403000000', '平川区');
INSERT INTO `j_position_county` VALUES ('2619', '620400000000', '620421000000', '靖远县');
INSERT INTO `j_position_county` VALUES ('2620', '620400000000', '620422000000', '会宁县');
INSERT INTO `j_position_county` VALUES ('2621', '620400000000', '620423000000', '景泰县');
INSERT INTO `j_position_county` VALUES ('2622', '620500000000', '620502000000', '秦州区');
INSERT INTO `j_position_county` VALUES ('2623', '620500000000', '620503000000', '麦积区');
INSERT INTO `j_position_county` VALUES ('2624', '620500000000', '620521000000', '清水县');
INSERT INTO `j_position_county` VALUES ('2625', '620500000000', '620522000000', '秦安县');
INSERT INTO `j_position_county` VALUES ('2626', '620500000000', '620523000000', '甘谷县');
INSERT INTO `j_position_county` VALUES ('2627', '620500000000', '620524000000', '武山县');
INSERT INTO `j_position_county` VALUES ('2628', '620500000000', '620525000000', '张家川回族自治县');
INSERT INTO `j_position_county` VALUES ('2629', '620600000000', '620602000000', '凉州区');
INSERT INTO `j_position_county` VALUES ('2630', '620600000000', '620621000000', '民勤县');
INSERT INTO `j_position_county` VALUES ('2631', '620600000000', '620622000000', '古浪县');
INSERT INTO `j_position_county` VALUES ('2632', '620600000000', '620623000000', '天祝藏族自治县');
INSERT INTO `j_position_county` VALUES ('2633', '620700000000', '620702000000', '甘州区');
INSERT INTO `j_position_county` VALUES ('2634', '620700000000', '620721000000', '肃南裕固族自治县');
INSERT INTO `j_position_county` VALUES ('2635', '620700000000', '620722000000', '民乐县');
INSERT INTO `j_position_county` VALUES ('2636', '620700000000', '620723000000', '临泽县');
INSERT INTO `j_position_county` VALUES ('2637', '620700000000', '620724000000', '高台县');
INSERT INTO `j_position_county` VALUES ('2638', '620700000000', '620725000000', '山丹县');
INSERT INTO `j_position_county` VALUES ('2639', '620800000000', '620802000000', '崆峒区');
INSERT INTO `j_position_county` VALUES ('2640', '620800000000', '620821000000', '泾川县');
INSERT INTO `j_position_county` VALUES ('2641', '620800000000', '620822000000', '灵台县');
INSERT INTO `j_position_county` VALUES ('2642', '620800000000', '620823000000', '崇信县');
INSERT INTO `j_position_county` VALUES ('2643', '620800000000', '620824000000', '华亭县');
INSERT INTO `j_position_county` VALUES ('2644', '620800000000', '620825000000', '庄浪县');
INSERT INTO `j_position_county` VALUES ('2645', '620800000000', '620826000000', '静宁县');
INSERT INTO `j_position_county` VALUES ('2646', '620900000000', '620902000000', '肃州区');
INSERT INTO `j_position_county` VALUES ('2647', '620900000000', '620921000000', '金塔县');
INSERT INTO `j_position_county` VALUES ('2648', '620900000000', '620922000000', '瓜州县');
INSERT INTO `j_position_county` VALUES ('2649', '620900000000', '620923000000', '肃北蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('2650', '620900000000', '620924000000', '阿克塞哈萨克族自治县');
INSERT INTO `j_position_county` VALUES ('2651', '620900000000', '620981000000', '玉门市');
INSERT INTO `j_position_county` VALUES ('2652', '620900000000', '620982000000', '敦煌市');
INSERT INTO `j_position_county` VALUES ('2653', '621000000000', '621002000000', '西峰区');
INSERT INTO `j_position_county` VALUES ('2654', '621000000000', '621021000000', '庆城县');
INSERT INTO `j_position_county` VALUES ('2655', '621000000000', '621022000000', '环县');
INSERT INTO `j_position_county` VALUES ('2656', '621000000000', '621023000000', '华池县');
INSERT INTO `j_position_county` VALUES ('2657', '621000000000', '621024000000', '合水县');
INSERT INTO `j_position_county` VALUES ('2658', '621000000000', '621025000000', '正宁县');
INSERT INTO `j_position_county` VALUES ('2659', '621000000000', '621026000000', '宁县');
INSERT INTO `j_position_county` VALUES ('2660', '621000000000', '621027000000', '镇原县');
INSERT INTO `j_position_county` VALUES ('2661', '621100000000', '621102000000', '安定区');
INSERT INTO `j_position_county` VALUES ('2662', '621100000000', '621121000000', '通渭县');
INSERT INTO `j_position_county` VALUES ('2663', '621100000000', '621122000000', '陇西县');
INSERT INTO `j_position_county` VALUES ('2664', '621100000000', '621123000000', '渭源县');
INSERT INTO `j_position_county` VALUES ('2665', '621100000000', '621124000000', '临洮县');
INSERT INTO `j_position_county` VALUES ('2666', '621100000000', '621125000000', '漳县');
INSERT INTO `j_position_county` VALUES ('2667', '621100000000', '621126000000', '岷县');
INSERT INTO `j_position_county` VALUES ('2668', '621200000000', '621202000000', '武都区');
INSERT INTO `j_position_county` VALUES ('2669', '621200000000', '621221000000', '成县');
INSERT INTO `j_position_county` VALUES ('2670', '621200000000', '621222000000', '文县');
INSERT INTO `j_position_county` VALUES ('2671', '621200000000', '621223000000', '宕昌县');
INSERT INTO `j_position_county` VALUES ('2672', '621200000000', '621224000000', '康县');
INSERT INTO `j_position_county` VALUES ('2673', '621200000000', '621225000000', '西和县');
INSERT INTO `j_position_county` VALUES ('2674', '621200000000', '621226000000', '礼县');
INSERT INTO `j_position_county` VALUES ('2675', '621200000000', '621227000000', '徽县');
INSERT INTO `j_position_county` VALUES ('2676', '621200000000', '621228000000', '两当县');
INSERT INTO `j_position_county` VALUES ('2677', '622900000000', '622901000000', '临夏市');
INSERT INTO `j_position_county` VALUES ('2678', '622900000000', '622921000000', '临夏县');
INSERT INTO `j_position_county` VALUES ('2679', '622900000000', '622922000000', '康乐县');
INSERT INTO `j_position_county` VALUES ('2680', '622900000000', '622923000000', '永靖县');
INSERT INTO `j_position_county` VALUES ('2681', '622900000000', '622924000000', '广河县');
INSERT INTO `j_position_county` VALUES ('2682', '622900000000', '622925000000', '和政县');
INSERT INTO `j_position_county` VALUES ('2683', '622900000000', '622926000000', '东乡族自治县');
INSERT INTO `j_position_county` VALUES ('2684', '622900000000', '622927000000', '积石山保安族东乡族撒拉族自治县');
INSERT INTO `j_position_county` VALUES ('2685', '623000000000', '623001000000', '合作市');
INSERT INTO `j_position_county` VALUES ('2686', '623000000000', '623021000000', '临潭县');
INSERT INTO `j_position_county` VALUES ('2687', '623000000000', '623022000000', '卓尼县');
INSERT INTO `j_position_county` VALUES ('2688', '623000000000', '623023000000', '舟曲县');
INSERT INTO `j_position_county` VALUES ('2689', '623000000000', '623024000000', '迭部县');
INSERT INTO `j_position_county` VALUES ('2690', '623000000000', '623025000000', '玛曲县');
INSERT INTO `j_position_county` VALUES ('2691', '623000000000', '623026000000', '碌曲县');
INSERT INTO `j_position_county` VALUES ('2692', '623000000000', '623027000000', '夏河县');
INSERT INTO `j_position_county` VALUES ('2693', '630100000000', '630102000000', '城东区');
INSERT INTO `j_position_county` VALUES ('2694', '630100000000', '630103000000', '城中区');
INSERT INTO `j_position_county` VALUES ('2695', '630100000000', '630104000000', '城西区');
INSERT INTO `j_position_county` VALUES ('2696', '630100000000', '630105000000', '城北区');
INSERT INTO `j_position_county` VALUES ('2697', '630100000000', '630121000000', '大通回族土族自治县');
INSERT INTO `j_position_county` VALUES ('2698', '630100000000', '630122000000', '湟中县');
INSERT INTO `j_position_county` VALUES ('2699', '630100000000', '630123000000', '湟源县');
INSERT INTO `j_position_county` VALUES ('2700', '630200000000', '630202000000', '乐都区');
INSERT INTO `j_position_county` VALUES ('2701', '630200000000', '630221000000', '平安县');
INSERT INTO `j_position_county` VALUES ('2702', '630200000000', '630222000000', '民和回族土族自治县');
INSERT INTO `j_position_county` VALUES ('2703', '630200000000', '630223000000', '互助土族自治县');
INSERT INTO `j_position_county` VALUES ('2704', '630200000000', '630224000000', '化隆回族自治县');
INSERT INTO `j_position_county` VALUES ('2705', '630200000000', '630225000000', '循化撒拉族自治县');
INSERT INTO `j_position_county` VALUES ('2706', '632200000000', '632221000000', '门源回族自治县');
INSERT INTO `j_position_county` VALUES ('2707', '632200000000', '632222000000', '祁连县');
INSERT INTO `j_position_county` VALUES ('2708', '632200000000', '632223000000', '海晏县');
INSERT INTO `j_position_county` VALUES ('2709', '632200000000', '632224000000', '刚察县');
INSERT INTO `j_position_county` VALUES ('2710', '632300000000', '632321000000', '同仁县');
INSERT INTO `j_position_county` VALUES ('2711', '632300000000', '632322000000', '尖扎县');
INSERT INTO `j_position_county` VALUES ('2712', '632300000000', '632323000000', '泽库县');
INSERT INTO `j_position_county` VALUES ('2713', '632300000000', '632324000000', '河南蒙古族自治县');
INSERT INTO `j_position_county` VALUES ('2714', '632500000000', '632521000000', '共和县');
INSERT INTO `j_position_county` VALUES ('2715', '632500000000', '632522000000', '同德县');
INSERT INTO `j_position_county` VALUES ('2716', '632500000000', '632523000000', '贵德县');
INSERT INTO `j_position_county` VALUES ('2717', '632500000000', '632524000000', '兴海县');
INSERT INTO `j_position_county` VALUES ('2718', '632500000000', '632525000000', '贵南县');
INSERT INTO `j_position_county` VALUES ('2719', '632600000000', '632621000000', '玛沁县');
INSERT INTO `j_position_county` VALUES ('2720', '632600000000', '632622000000', '班玛县');
INSERT INTO `j_position_county` VALUES ('2721', '632600000000', '632623000000', '甘德县');
INSERT INTO `j_position_county` VALUES ('2722', '632600000000', '632624000000', '达日县');
INSERT INTO `j_position_county` VALUES ('2723', '632600000000', '632625000000', '久治县');
INSERT INTO `j_position_county` VALUES ('2724', '632600000000', '632626000000', '玛多县');
INSERT INTO `j_position_county` VALUES ('2725', '632700000000', '632701000000', '玉树市');
INSERT INTO `j_position_county` VALUES ('2726', '632700000000', '632722000000', '杂多县');
INSERT INTO `j_position_county` VALUES ('2727', '632700000000', '632723000000', '称多县');
INSERT INTO `j_position_county` VALUES ('2728', '632700000000', '632724000000', '治多县');
INSERT INTO `j_position_county` VALUES ('2729', '632700000000', '632725000000', '囊谦县');
INSERT INTO `j_position_county` VALUES ('2730', '632700000000', '632726000000', '曲麻莱县');
INSERT INTO `j_position_county` VALUES ('2731', '632800000000', '632801000000', '格尔木市');
INSERT INTO `j_position_county` VALUES ('2732', '632800000000', '632802000000', '德令哈市');
INSERT INTO `j_position_county` VALUES ('2733', '632800000000', '632821000000', '乌兰县');
INSERT INTO `j_position_county` VALUES ('2734', '632800000000', '632822000000', '都兰县');
INSERT INTO `j_position_county` VALUES ('2735', '632800000000', '632823000000', '天峻县');
INSERT INTO `j_position_county` VALUES ('2736', '640100000000', '640104000000', '兴庆区');
INSERT INTO `j_position_county` VALUES ('2737', '640100000000', '640105000000', '西夏区');
INSERT INTO `j_position_county` VALUES ('2738', '640100000000', '640106000000', '金凤区');
INSERT INTO `j_position_county` VALUES ('2739', '640100000000', '640121000000', '永宁县');
INSERT INTO `j_position_county` VALUES ('2740', '640100000000', '640122000000', '贺兰县');
INSERT INTO `j_position_county` VALUES ('2741', '640100000000', '640181000000', '灵武市');
INSERT INTO `j_position_county` VALUES ('2742', '640200000000', '640202000000', '大武口区');
INSERT INTO `j_position_county` VALUES ('2743', '640200000000', '640205000000', '惠农区');
INSERT INTO `j_position_county` VALUES ('2744', '640200000000', '640221000000', '平罗县');
INSERT INTO `j_position_county` VALUES ('2745', '640300000000', '640302000000', '利通区');
INSERT INTO `j_position_county` VALUES ('2746', '640300000000', '640303000000', '红寺堡区');
INSERT INTO `j_position_county` VALUES ('2747', '640300000000', '640323000000', '盐池县');
INSERT INTO `j_position_county` VALUES ('2748', '640300000000', '640324000000', '同心县');
INSERT INTO `j_position_county` VALUES ('2749', '640300000000', '640381000000', '青铜峡市');
INSERT INTO `j_position_county` VALUES ('2750', '640400000000', '640402000000', '原州区');
INSERT INTO `j_position_county` VALUES ('2751', '640400000000', '640422000000', '西吉县');
INSERT INTO `j_position_county` VALUES ('2752', '640400000000', '640423000000', '隆德县');
INSERT INTO `j_position_county` VALUES ('2753', '640400000000', '640424000000', '泾源县');
INSERT INTO `j_position_county` VALUES ('2754', '640400000000', '640425000000', '彭阳县');
INSERT INTO `j_position_county` VALUES ('2755', '640500000000', '640502000000', '沙坡头区');
INSERT INTO `j_position_county` VALUES ('2756', '640500000000', '640521000000', '中宁县');
INSERT INTO `j_position_county` VALUES ('2757', '640500000000', '640522000000', '海原县');
INSERT INTO `j_position_county` VALUES ('2758', '650100000000', '650102000000', '天山区');
INSERT INTO `j_position_county` VALUES ('2759', '650100000000', '650103000000', '沙依巴克区');
INSERT INTO `j_position_county` VALUES ('2760', '650100000000', '650104000000', '新市区');
INSERT INTO `j_position_county` VALUES ('2761', '650100000000', '650105000000', '水磨沟区');
INSERT INTO `j_position_county` VALUES ('2762', '650100000000', '650106000000', '头屯河区');
INSERT INTO `j_position_county` VALUES ('2763', '650100000000', '650107000000', '达坂城区');
INSERT INTO `j_position_county` VALUES ('2764', '650100000000', '650109000000', '米东区');
INSERT INTO `j_position_county` VALUES ('2765', '650100000000', '650121000000', '乌鲁木齐县');
INSERT INTO `j_position_county` VALUES ('2766', '650200000000', '650202000000', '独山子区');
INSERT INTO `j_position_county` VALUES ('2767', '650200000000', '650203000000', '克拉玛依区');
INSERT INTO `j_position_county` VALUES ('2768', '650200000000', '650204000000', '白碱滩区');
INSERT INTO `j_position_county` VALUES ('2769', '650200000000', '650205000000', '乌尔禾区');
INSERT INTO `j_position_county` VALUES ('2770', '652100000000', '652101000000', '吐鲁番市');
INSERT INTO `j_position_county` VALUES ('2771', '652100000000', '652122000000', '鄯善县');
INSERT INTO `j_position_county` VALUES ('2772', '652100000000', '652123000000', '托克逊县');
INSERT INTO `j_position_county` VALUES ('2773', '652200000000', '652201000000', '哈密市');
INSERT INTO `j_position_county` VALUES ('2774', '652200000000', '652222000000', '巴里坤哈萨克自治县');
INSERT INTO `j_position_county` VALUES ('2775', '652200000000', '652223000000', '伊吾县');
INSERT INTO `j_position_county` VALUES ('2776', '652300000000', '652301000000', '昌吉市');
INSERT INTO `j_position_county` VALUES ('2777', '652300000000', '652302000000', '阜康市');
INSERT INTO `j_position_county` VALUES ('2778', '652300000000', '652323000000', '呼图壁县');
INSERT INTO `j_position_county` VALUES ('2779', '652300000000', '652324000000', '玛纳斯县');
INSERT INTO `j_position_county` VALUES ('2780', '652300000000', '652325000000', '奇台县');
INSERT INTO `j_position_county` VALUES ('2781', '652300000000', '652327000000', '吉木萨尔县');
INSERT INTO `j_position_county` VALUES ('2782', '652300000000', '652328000000', '木垒哈萨克自治县');
INSERT INTO `j_position_county` VALUES ('2783', '652700000000', '652701000000', '博乐市');
INSERT INTO `j_position_county` VALUES ('2784', '652700000000', '652702000000', '阿拉山口市');
INSERT INTO `j_position_county` VALUES ('2785', '652700000000', '652722000000', '精河县');
INSERT INTO `j_position_county` VALUES ('2786', '652700000000', '652723000000', '温泉县');
INSERT INTO `j_position_county` VALUES ('2787', '652800000000', '652801000000', '库尔勒市');
INSERT INTO `j_position_county` VALUES ('2788', '652800000000', '652822000000', '轮台县');
INSERT INTO `j_position_county` VALUES ('2789', '652800000000', '652823000000', '尉犁县');
INSERT INTO `j_position_county` VALUES ('2790', '652800000000', '652824000000', '若羌县');
INSERT INTO `j_position_county` VALUES ('2791', '652800000000', '652825000000', '且末县');
INSERT INTO `j_position_county` VALUES ('2792', '652800000000', '652826000000', '焉耆回族自治县');
INSERT INTO `j_position_county` VALUES ('2793', '652800000000', '652827000000', '和静县');
INSERT INTO `j_position_county` VALUES ('2794', '652800000000', '652828000000', '和硕县');
INSERT INTO `j_position_county` VALUES ('2795', '652800000000', '652829000000', '博湖县');
INSERT INTO `j_position_county` VALUES ('2796', '652900000000', '652901000000', '阿克苏市');
INSERT INTO `j_position_county` VALUES ('2797', '652900000000', '652922000000', '温宿县');
INSERT INTO `j_position_county` VALUES ('2798', '652900000000', '652923000000', '库车县');
INSERT INTO `j_position_county` VALUES ('2799', '652900000000', '652924000000', '沙雅县');
INSERT INTO `j_position_county` VALUES ('2800', '652900000000', '652925000000', '新和县');
INSERT INTO `j_position_county` VALUES ('2801', '652900000000', '652926000000', '拜城县');
INSERT INTO `j_position_county` VALUES ('2802', '652900000000', '652927000000', '乌什县');
INSERT INTO `j_position_county` VALUES ('2803', '652900000000', '652928000000', '阿瓦提县');
INSERT INTO `j_position_county` VALUES ('2804', '652900000000', '652929000000', '柯坪县');
INSERT INTO `j_position_county` VALUES ('2805', '653000000000', '653001000000', '阿图什市');
INSERT INTO `j_position_county` VALUES ('2806', '653000000000', '653022000000', '阿克陶县');
INSERT INTO `j_position_county` VALUES ('2807', '653000000000', '653023000000', '阿合奇县');
INSERT INTO `j_position_county` VALUES ('2808', '653000000000', '653024000000', '乌恰县');
INSERT INTO `j_position_county` VALUES ('2809', '653100000000', '653101000000', '喀什市');
INSERT INTO `j_position_county` VALUES ('2810', '653100000000', '653121000000', '疏附县');
INSERT INTO `j_position_county` VALUES ('2811', '653100000000', '653122000000', '疏勒县');
INSERT INTO `j_position_county` VALUES ('2812', '653100000000', '653123000000', '英吉沙县');
INSERT INTO `j_position_county` VALUES ('2813', '653100000000', '653124000000', '泽普县');
INSERT INTO `j_position_county` VALUES ('2814', '653100000000', '653125000000', '莎车县');
INSERT INTO `j_position_county` VALUES ('2815', '653100000000', '653126000000', '叶城县');
INSERT INTO `j_position_county` VALUES ('2816', '653100000000', '653127000000', '麦盖提县');
INSERT INTO `j_position_county` VALUES ('2817', '653100000000', '653128000000', '岳普湖县');
INSERT INTO `j_position_county` VALUES ('2818', '653100000000', '653129000000', '伽师县');
INSERT INTO `j_position_county` VALUES ('2819', '653100000000', '653130000000', '巴楚县');
INSERT INTO `j_position_county` VALUES ('2820', '653100000000', '653131000000', '塔什库尔干塔吉克自治县');
INSERT INTO `j_position_county` VALUES ('2821', '653200000000', '653201000000', '和田市');
INSERT INTO `j_position_county` VALUES ('2822', '653200000000', '653221000000', '和田县');
INSERT INTO `j_position_county` VALUES ('2823', '653200000000', '653222000000', '墨玉县');
INSERT INTO `j_position_county` VALUES ('2824', '653200000000', '653223000000', '皮山县');
INSERT INTO `j_position_county` VALUES ('2825', '653200000000', '653224000000', '洛浦县');
INSERT INTO `j_position_county` VALUES ('2826', '653200000000', '653225000000', '策勒县');
INSERT INTO `j_position_county` VALUES ('2827', '653200000000', '653226000000', '于田县');
INSERT INTO `j_position_county` VALUES ('2828', '653200000000', '653227000000', '民丰县');
INSERT INTO `j_position_county` VALUES ('2829', '654000000000', '654002000000', '伊宁市');
INSERT INTO `j_position_county` VALUES ('2830', '654000000000', '654003000000', '奎屯市');
INSERT INTO `j_position_county` VALUES ('2831', '654000000000', '654021000000', '伊宁县');
INSERT INTO `j_position_county` VALUES ('2832', '654000000000', '654022000000', '察布查尔锡伯自治县');
INSERT INTO `j_position_county` VALUES ('2833', '654000000000', '654023000000', '霍城县');
INSERT INTO `j_position_county` VALUES ('2834', '654000000000', '654024000000', '巩留县');
INSERT INTO `j_position_county` VALUES ('2835', '654000000000', '654025000000', '新源县');
INSERT INTO `j_position_county` VALUES ('2836', '654000000000', '654026000000', '昭苏县');
INSERT INTO `j_position_county` VALUES ('2837', '654000000000', '654027000000', '特克斯县');
INSERT INTO `j_position_county` VALUES ('2838', '654000000000', '654028000000', '尼勒克县');
INSERT INTO `j_position_county` VALUES ('2839', '654200000000', '654201000000', '塔城市');
INSERT INTO `j_position_county` VALUES ('2840', '654200000000', '654202000000', '乌苏市');
INSERT INTO `j_position_county` VALUES ('2841', '654200000000', '654221000000', '额敏县');
INSERT INTO `j_position_county` VALUES ('2842', '654200000000', '654223000000', '沙湾县');
INSERT INTO `j_position_county` VALUES ('2843', '654200000000', '654224000000', '托里县');
INSERT INTO `j_position_county` VALUES ('2844', '654200000000', '654225000000', '裕民县');
INSERT INTO `j_position_county` VALUES ('2845', '654200000000', '654226000000', '和布克赛尔蒙古自治县');
INSERT INTO `j_position_county` VALUES ('2846', '654300000000', '654301000000', '阿勒泰市');
INSERT INTO `j_position_county` VALUES ('2847', '654300000000', '654321000000', '布尔津县');
INSERT INTO `j_position_county` VALUES ('2848', '654300000000', '654322000000', '富蕴县');
INSERT INTO `j_position_county` VALUES ('2849', '654300000000', '654323000000', '福海县');
INSERT INTO `j_position_county` VALUES ('2850', '654300000000', '654324000000', '哈巴河县');
INSERT INTO `j_position_county` VALUES ('2851', '654300000000', '654325000000', '青河县');
INSERT INTO `j_position_county` VALUES ('2852', '654300000000', '654326000000', '吉木乃县');
INSERT INTO `j_position_county` VALUES ('2853', '659000000000', '659001000000', '石河子市');
INSERT INTO `j_position_county` VALUES ('2854', '659000000000', '659002000000', '阿拉尔市');
INSERT INTO `j_position_county` VALUES ('2855', '659000000000', '659003000000', '图木舒克市');
INSERT INTO `j_position_county` VALUES ('2856', '659000000000', '659004000000', '五家渠市');
      
*/

/* === (21) ap_status === */
SELECT '-----ap_status prcess--------';

CREATE TABLE ap_status 
(  
	id             int      unsigned     primary key NOT NULL auto_increment COMMENT '主键ID',
	SCTP           tinyint      	     NULL  default 0 COMMENT 'SCTP连接状态：1,正常；0，不正常',
	S1             tinyint      	     NULL  default 0 COMMENT 'S1连接状态  ：1,正常；0，不正常',
	GPS            tinyint      	     NULL  default 0 COMMENT 'GPS连接状态 ：1,正常；0，不正常',
	CELL           tinyint      	     NULL  default 0 COMMENT 'CELL状态    ：1,正常；0，不正常',
	SYNC           tinyint      	     NULL  default 0 COMMENT '同步状态    ：1,正常；0，不正常',
	LICENSE        tinyint      	     NULL  default 0 COMMENT 'LICENSE状态 ：1,正常；0，不正常',
	RADIO          tinyint      	     NULL  default 0 COMMENT '射频状态    ：1,正常；0，不正常',
	time           datetime              NULL  default 0 COMMENT '时间戳',
	affDeviceId    int                   NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;



/* === (22) ap_general_para === */
SELECT '-----ap_general_para prcess--------';

CREATE TABLE ap_general_para 
(  
	id                 int      unsigned     primary key NOT NULL auto_increment COMMENT '主键ID',
	mode               char(16)      	     NULL  COMMENT '制式：GSM,TD-SCDMA,WCDMA,LTE-TDD,LTE-FDD',
	primaryplmn        char(16)      	     NULL  COMMENT '主plmn',
	earfcndl           char(16)      	     NULL  COMMENT '工作上行频点',
	earfcnul           char(16)      	     NULL  COMMENT '工作下行频点',
	cellid             char(16)      	     NULL  COMMENT 'cellid',       /*2018-06-26*/
	pci                char(16)      	     NULL  COMMENT '工作pci',
	bandwidth          char(16)      	     NULL  COMMENT '工作带宽',
	tac                smallint unsigned     NULL  COMMENT 'TAC',
	txpower            char(16)      	     NULL  COMMENT '功率衰减',
	periodtac          smallint      	     NULL  COMMENT 'TAC变化周期',
	manualfreq         tinyint      	     NULL  COMMENT '选频方式 0：自动选频 1：手动选频',
	bootMode           tinyint      	     NULL  COMMENT '设备启动方式 0：半自动 1：全自动',
	Earfcnlist         char(128)      	     NULL  COMMENT '频点列表，如：38950,39150',
	Bandoffset         char(128)      	     NULL  COMMENT '频偏":"39,70000;38,10000',
	NTP                char(16)      	     NULL  COMMENT 'NTP服务器ip',
	ntppri             tinyint      	     NULL  COMMENT 'Ntp的优先级',
	source             tinyint      	     NULL  COMMENT '同步源(0：GPS ； 1：CNM ； 2：no sync)',
	ManualEnable       tinyint      	     NULL  COMMENT '是否设定手动同步源',
	ManualEarfcn       int      	         NULL  COMMENT '手动设置同步频点',
	ManualPci          int      	         NULL  COMMENT '手动设置同步pci',
	ManualBw           int      	         NULL  COMMENT '手动设置同步带宽',	
	gpsConfig          tinyint      	     NULL  COMMENT 'GPS配置，0表示NOGPS，1表示GPS',
	activeTime1Start   time                  NULL  COMMENT '生效时间1的起始时间',
	activeTime1Ended   time                  NULL  COMMENT '生效时间1的结束时间',
	activeTime2Start   time                  NULL  COMMENT '生效时间2的起始时间',
	activeTime2Ended   time                  NULL  COMMENT '生效时间2的结束时间',
	activeTime3Start   time                  NULL  COMMENT '生效时间3的起始时间,有的话就添加该项',
	activeTime3Ended   time                  NULL  COMMENT '生效时间3的结束时间,有的话就添加该项',
	activeTime4Start   time                  NULL  COMMENT '生效时间4的起始时间,有的话就添加该项',
	activeTime4Ended   time                  NULL  COMMENT '生效时间4的结束时间,有的话就添加该项',
	time               datetime              NULL  COMMENT '更新时间戳',
	affDeviceId        int                   NOT NULL COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;


/* === (23) update_info === */
SELECT '-----update_info prcess--------';

CREATE TABLE update_info 
(  
	id             int                   primary key NOT NULL auto_increment COMMENT '主键ID',
	md5sum         char(64)      	     NULL  default 0 COMMENT '文件的MD5校验和',
	fileName       char(255)      	     NULL  default 0 COMMENT '升级包文件名',	
	version        char(255)      	     NULL  default 0 COMMENT '升级包版本号',	
	time           datetime              NULL  default 0 COMMENT '时间'
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;

CREATE INDEX inx_update_info ON update_info (md5sum(16));


/* === (24) redirection === */
SELECT '-----redirection prcess--------';

CREATE TABLE redirection 
(  
	id               int        unsigned     primary key NOT  NULL auto_increment COMMENT '主键ID',
	category         tinyint    unsigned     NULL  default 0  COMMENT '0:white,1:black,2:other',
	priority         tinyint    unsigned     NULL  default 0  COMMENT '2:2G,3:3G,4:4G,Others:noredirect',
	GeranRedirect    tinyint    unsigned     NULL  default 0  COMMENT '0:disable;1:enable',
	arfcn            int        unsigned     NULL  default 0  COMMENT '2G frequency',
	UtranRedirect    tinyint    unsigned     NULL  default 0  COMMENT '0:disable;1:enable',
	uarfcn           int        unsigned     NULL  default 0  COMMENT '3G frequency',
	EutranRedirect   tinyint    unsigned     NULL  default 0  COMMENT '0:disable;1:enable',
	earfcn           int        unsigned     NULL  default 0  COMMENT '4G frequency',
	RejectMethod     tinyint    unsigned     NULL  default 0  COMMENT '1,2,0xFF,0x10-0xFE',
	additionalFreq   char(255)      	     NULL  default '' COMMENT 'uarfcn,uarfcn;不超过7个freq，超过7个freq的默认丢弃',	
	affDeviceId      int                     NOT NULL  COMMENT '所属的设备ID号；即关联到deviceinfo表 外键FK'	
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;

CREATE INDEX inx_redirection ON redirection (category,affDeviceId);

DELIMITER ;;
CREATE PROCEDURE `generate_test_data`()
BEGIN

    INSERT INTO privilege VALUES(1,'查看日志'      ,'查看日志'      ,'查看日志');
	INSERT INTO privilege VALUES(2,'搜索黑名单'    ,'搜索黑名单'    ,'搜索黑名单');
	INSERT INTO privilege VALUES(3,'添加用户'      ,'添加用户'      ,'添加用户');
	INSERT INTO privilege VALUES(4,'添加设备'      ,'添加设备'      ,'添加设备');
	INSERT INTO privilege VALUES(5,'访问西北监控'  ,'访问西北监控'  ,'访问西北监控');
	
	/* INSERT INTO domain VALUES(1,'设备',-1,'设备',0,'根节点(默认存在)'); */
	
	INSERT INTO domain VALUES(2,'深圳',1,'设备.深圳',0,'11');
	INSERT INTO domain VALUES(3,'东莞',1,'设备.东莞',0,'22');	
	INSERT INTO domain VALUES(4,'福田',2,'设备.深圳.福田',0,'33');
	INSERT INTO domain VALUES(5,'南山',2,'设备.深圳.南山',0,'44');	
	INSERT INTO domain VALUES(6,'城区',3,'设备.东莞.城区',1,'55');	
	INSERT INTO domain VALUES(7,'中心广场',4,'设备.深圳.福田.中心广场',0,'66');
	INSERT INTO domain VALUES(8,'莲花山'  ,4,'设备.深圳.福田.莲花山',  0,'77');	
	INSERT INTO domain VALUES(9,'西北监控',7,'设备.深圳.福田.中心广场.西北监控',1,'88');
	INSERT INTO domain VALUES(10,'device0',8,'设备.深圳.福田.莲花山.device0',1,'device0');	
	
	INSERT INTO device VALUES(1,'电信FDD','EN1800S116340039','172.17.0.123',12345,'255.255.255.0','FDD',  0,NOW(),1,NULL,9);
	INSERT INTO device VALUES(2,'移动TDD','EN1800S116340040','172.17.0.124',12346,'255.255.255.0','TDD',  0,NOW(),1,NULL,9);
	INSERT INTO device VALUES(3,'联通W',  'EN1800S116340041','172.17.0.125',12347,'255.255.255.0','WCDMA',0,NOW(),1,NULL,9);
	INSERT INTO device VALUES(4,'移动GSM','EN1800S116340042','172.17.0.126',12348,'255.255.255.0','GSM',  0,NOW(),1,NULL,9);	
	INSERT INTO device VALUES(5,'电信FDD','EN1800S116340043','172.17.0.127',12349,'255.255.255.0','FDD',  0,NOW(),1,NULL,6);
	INSERT INTO device VALUES(6,'移动TDD','EN1800S116340044','172.17.0.128',12350,'255.255.255.0','TDD',  0,NOW(),1,NULL,6);
	INSERT INTO device VALUES(7,'联通W',  'EN1800S116340045','172.17.0.129',12351,'255.255.255.0','WCDMA',0,NOW(),1,NULL,6);
	INSERT INTO device VALUES(8,'移动GSM','EN1800S116340046','172.17.0.130',12352,'255.255.255.0','GSM',  0,NOW(),1,NULL,6);
	
	INSERT INTO device VALUES(9, 'board0','EN1801E123456789','172.17.0.131',12352,'255.255.255.0','FDD',  0,NOW(),1,NULL,10);
	INSERT INTO device VALUES(10,'board1','EN1801E123456790','172.17.0.132',12352,'255.255.255.0','FDD',  0,NOW(),1,NULL,10);
	INSERT INTO device VALUES(11,'board2','EN1801E123456791','172.17.0.133',12352,'255.255.255.0','FDD',  0,NOW(),1,NULL,10);
	INSERT INTO device VALUES(12,'board3','EN1801E123456792','172.17.0.134',12352,'255.255.255.0','FDD',  0,NOW(),1,NULL,10);
	
	INSERT INTO ap_status VALUES(1,0,0,0,0,0,0,0,NOW(),1);
	INSERT INTO ap_status VALUES(2,0,0,0,0,0,0,0,NOW(),2);
	INSERT INTO ap_status VALUES(3,0,0,0,0,0,0,0,NOW(),3);
	INSERT INTO ap_status VALUES(4,0,0,0,0,0,0,0,NOW(),4);
	INSERT INTO ap_status VALUES(5,0,0,0,0,0,0,0,NOW(),5);
	INSERT INTO ap_status VALUES(6,0,0,0,0,0,0,0,NOW(),6);
	INSERT INTO ap_status VALUES(7,0,0,0,0,0,0,0,NOW(),7);
	INSERT INTO ap_status VALUES(8,0,0,0,0,0,0,0,NOW(),8);
	
	INSERT INTO ap_status VALUES(9,0,0,0,0,0,0,0,NOW(),9);
	INSERT INTO ap_status VALUES(10,0,0,0,0,0,0,0,NOW(),10);
	INSERT INTO ap_status VALUES(11,0,0,0,0,0,0,0,NOW(),11);
	INSERT INTO ap_status VALUES(12,0,0,0,0,0,0,0,NOW(),12);
	
	INSERT INTO ap_general_para VALUES(1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),1);
	INSERT INTO ap_general_para VALUES(2,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),2);	
	INSERT INTO ap_general_para VALUES(3,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),3);
	INSERT INTO ap_general_para VALUES(4,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),4);	
	INSERT INTO ap_general_para VALUES(5,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),5);
	INSERT INTO ap_general_para VALUES(6,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),6);
	INSERT INTO ap_general_para VALUES(7,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),7);
	INSERT INTO ap_general_para VALUES(8,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),8);	
	
	INSERT INTO ap_general_para VALUES(9, NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),9);	
	INSERT INTO ap_general_para VALUES(10,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),10);	
	INSERT INTO ap_general_para VALUES(11,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),11);	
	INSERT INTO ap_general_para VALUES(12,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NOW(),12);	
	
	INSERT INTO roleprivilege VALUES(1,'RoleEng'  ,'1,2,3,4,5','RoleEng-des');
	INSERT INTO roleprivilege VALUES(2,'RoleSA'   ,'1,2,3,4,5','RoleSA-des');
	INSERT INTO roleprivilege VALUES(3,'RoleAdmin','1,2,3,4','RoleAdmin-des');
	INSERT INTO roleprivilege VALUES(4,'RoleSO'   ,'1,2,3','RoleSO-des');
	INSERT INTO roleprivilege VALUES(5,'RoleOP'   ,'1,2','RoleOP-des');
	
	INSERT INTO userdomain VALUES(1,'engi','6,9','userdomain-des');
	INSERT INTO userdomain VALUES(2,'root','9','userdomain-des');
	
END;;
DELIMITER ;
	

DELIMITER ;;
CREATE PROCEDURE `del_test_data`()
BEGIN

	DELETE FROM privilege;
	DELETE FROM domain;
	DELETE FROM device;
	DELETE FROM ap_status;
	DELETE FROM roleprivilege;
	DELETE FROM userdomain;
	
END;;
DELIMITER ;


DELIMITER ;;
CREATE PROCEDURE `delete_capture`()
BEGIN

    DELETE FROM capture;
	
END;;
DELIMITER ;