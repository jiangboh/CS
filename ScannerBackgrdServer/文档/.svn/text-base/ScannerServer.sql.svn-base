
/*******************************************************************************
   
     (1) modify the password of root
     (2) enable root to log DB in remotely
     (3) create DB of scanner_server
     (4) create various tables.
      
                            2018-04-24
                                                                                                                                          
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

INSERT INTO user VALUES(NULL,'engi','MD5(engi)','engi',NOW());
INSERT INTO user VALUES(NULL,'root','MD5(root)','root',NOW());



/* === (2) roletype === */
SELECT '-----roletype prcess--------';


DROP TABLE IF EXISTS roletype;
CREATE TABLE roletype 
(  
	id          tinyint       unsigned  primary key NOT NULL auto_increment COMMENT '主键ID',  
	roleType    char(64)      NOT NULL  COMMENT '角色类型，默认有Engineering,SuperAdmin,Administrator,SeniorOperator,Operator,还可以自定义',  
	des         varchar(256)  NULL default 'des' COMMENT '描述'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 

INSERT INTO roletype VALUES(1,'Engineering'   ,'Engineering');
INSERT INTO roletype VALUES(2,'SuperAdmin'    ,'SuperAdmin');
INSERT INTO roletype VALUES(3,'Administrator' ,'Administrator');
INSERT INTO roletype VALUES(4,'SeniorOperator','SeniorOperator');
INSERT INTO roletype VALUES(5,'Operator'      ,'Operator');


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

	                
INSERT INTO role VALUES(NULL,'RoleEng'  ,'Engineering'   ,'1970-1-1','3000-1-1','RoleEng');
INSERT INTO role VALUES(NULL,'RoleSA'   ,'SuperAdmin'    ,'1970-1-1','3000-1-1','RoleSA');
INSERT INTO role VALUES(NULL,'RoleAdmin','Administrator' ,'1970-1-1','3000-1-1','RoleAdmin');
INSERT INTO role VALUES(NULL,'RoleSO'   ,'SeniorOperator','1970-1-1','3000-1-1','RoleSO');
INSERT INTO role VALUES(NULL,'RoleOP'   ,'Operator'      ,'1970-1-1','3000-1-1','RoleOP');



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

INSERT INTO privilege VALUES(1,'查看日志'      ,'查看日志'      ,'查看日志');
INSERT INTO privilege VALUES(2,'搜索黑名单'    ,'搜索黑名单'    ,'搜索黑名单');
INSERT INTO privilege VALUES(3,'添加用户'      ,'添加用户'      ,'添加用户');
INSERT INTO privilege VALUES(4,'添加设备'      ,'添加设备'      ,'添加设备');
INSERT INTO privilege VALUES(5,'访问西北监控'  ,'访问西北监控'  ,'访问西北监控');



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

INSERT INTO userrole VALUES(1,'engi' ,'RoleEng' ,'engi->RoleEng');
INSERT INTO userrole VALUES(2,'root' ,'RoleSA'  ,'root->RoleSA');        


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
 
 
INSERT INTO domain VALUES(NULL,'设备',-1,'设备',0,'默认的根节点');



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
	id             int                   unsigned     primary key NOT NULL auto_increment COMMENT '主键ID',
	name           char(64)  	         NOT NULL  COMMENT '设备名称',
	sn             char(32)  	         NULL default '' COMMENT 'SN，GSM或第三方设备可能没有该字段',
	carrier        tinyint               NULL COMMENT '仅用于标识GSM的载波，0：载波0；1：载波1',
	bindingDevId   int                   unsigned     NULL COMMENT '仅用于标识GSM的绑定设备id', 
	ipAddr         char(32)  	         NULL COMMENT 'IP地址',
	port           smallint              unsigned  NULL COMMENT '端口号',
	netmask        char(16)  	         NULL COMMENT '掩码',
	mode           char(16)              NULL COMMENT '设备制式，LTE-TDD，LTE-FDD，GSM，WCDMA等', 
	online         tinyint      	     NULL default 0  COMMENT '上下线标识，0：下线；1：上线',
	lastOnline     datetime              NULL COMMENT '最后的上线时间',
	isActive       tinyint      	     NULL default 1  COMMENT '标识该设备是否生效，0：无效；1：生效',
	affDomainId    int              	 NOT NULL COMMENT '标识设备的从属于那个域，FK'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 




/* === (10) bwlist === */
SELECT '-----bwlist prcess--------';

CREATE TABLE bwlist 
(  
	id             int      unsigned     primary key NOT NULL auto_increment COMMENT '主键ID',
	imsi           char(15)  	         NULL  COMMENT 'IMSI号',
	imei           char(15)  	         NULL  COMMENT 'IMEI号',
	bwFlag         enum('black','white','other') DEFAULT 'other' COMMENT '名单类型标识',
	rbStart        tinyint      	     NULL default 0  COMMENT '起始RB',
	rbEnd          tinyint      	     NULL default 0  COMMENT '结束RB',
	time           datetime              NULL COMMENT '设置时间',
	des            varchar(128)  	     NULL default '' COMMENT '描述',
	linkFlag       tinyint      	     NOT NULL default 0  COMMENT '0：链接到DeviceId,1：链接到DomainId',
	affDeviceId    int                   NULL COMMENT '所属的设备ID号；即关联到device表',
	affDomainId    int              	 NULL COMMENT '标识设备的从属于那个域，FK' 
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;