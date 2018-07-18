
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
	usrId       int           unsigned    primary key NOT NULL auto_increment COMMENT '����ID',  
	name        char(64)      NOT NULL  COMMENT '�û���',  
	psw         BLOB          NOT NULL  COMMENT '�û�����',
	des         varchar(256)  NULL default '' COMMENT '����',
	operTime    datetime      NOT NULL  COMMENT '����ʱ������һ�εĵ�¼ʱ��'
 
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
	id          tinyint       unsigned  primary key NOT NULL auto_increment COMMENT '����ID',  
	roleType    char(64)      NOT NULL  COMMENT '��ɫ���ͣ�Ĭ����Engineering,SuperAdmin,Administrator,SeniorOperator,Operator,�������Զ���',  
	des         varchar(256)  NULL default 'des' COMMENT '����'
 
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
	roleId      int           unsigned  primary key NOT NULL auto_increment COMMENT '����ID',  
	name        char(64)      NOT NULL  COMMENT '��ɫ����',  
	roleType    char(64)      NOT NULL  COMMENT '��ɫ���ͣ�Ĭ����Engineering,SuperAdmin,Administrator,SeniorOperator,Operator,�������Զ���',  
	timeStart   datetime      NOT NULL  COMMENT '��ʼ����Чʱ��',
	timeEnd     datetime      NOT NULL  COMMENT '��������Чʱ��',
	des         varchar(256)  NULL default 'des' COMMENT '����'
 
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
	priId        int            unsigned primary key NOT NULL auto_increment COMMENT '����ID',
	funName      char(64)       NOT NULL COMMENT '��������',
	aliasName    char(64)       NOT NULL COMMENT '���ܱ���',	
	des          varchar(256)   NULL default 'des' COMMENT '����'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;      

INSERT INTO privilege VALUES(1,'�鿴��־'      ,'�鿴��־'      ,'�鿴��־');
INSERT INTO privilege VALUES(2,'����������'    ,'����������'    ,'����������');
INSERT INTO privilege VALUES(3,'����û�'      ,'����û�'      ,'����û�');
INSERT INTO privilege VALUES(4,'����豸'      ,'����豸'      ,'����豸');
INSERT INTO privilege VALUES(5,'�����������'  ,'�����������'  ,'�����������');



/* === (5) userrole === */
SELECT '-----userrole prcess--------';


DROP TABLE IF EXISTS userrole;
CREATE TABLE userrole
(
	usrRoleId    int            unsigned  primary key NOT NULL auto_increment COMMENT '����ID',
	usrName      char(64)       NOT NULL COMMENT '�û�����FK',
	roleName     char(64)       NOT NULL COMMENT '��ɫ����FK',	
	des          varchar(256)   NULL default 'des' COMMENT '����'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;   

INSERT INTO userrole VALUES(1,'engi' ,'RoleEng' ,'engi->RoleEng');
INSERT INTO userrole VALUES(2,'root' ,'RoleSA'  ,'root->RoleSA');        


/* === (6) roleprivilege === */
SELECT '-----roleprivilege prcess--------';


DROP TABLE IF EXISTS roleprivilege;
CREATE TABLE roleprivilege
(
	rolePriId    int            unsigned  primary key NOT NULL auto_increment COMMENT '����ID',
	roleName     char(64)       NOT NULL COMMENT '��ɫ����FK',	
	priIdSet     varchar(1024)  NULL default '' COMMENT 'Ȩ��ID����',	
	des          varchar(256)   NULL default 'des' COMMENT '����'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;



/* === (7) domain === */
SELECT '-----domain prcess--------';

DROP TABLE IF EXISTS domain; 
CREATE TABLE domain
(
	id            int            primary key NOT NULL auto_increment COMMENT '����ID',
	name          char(64)       NOT NULL COMMENT '�ڵ������',	
	parentId      int            NOT NULL COMMENT '�ڵ�ĸ���ID',	
	nameFullPath  varchar(1024)  NOT NULL COMMENT '�ڵ������ȫ·��',	
	isStation     tinyint        NOT NULL COMMENT '��ʶ�Ƿ�Ϊվ��',	
	des           varchar(256)   NULL default 'des' COMMENT '����'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;
 
 
INSERT INTO domain VALUES(NULL,'�豸',-1,'�豸',0,'Ĭ�ϵĸ��ڵ�');



/* === (8) userdomain === */
SELECT '-----userdomain prcess--------';

/* DROP TABLE IF EXISTS userdomain; */
CREATE TABLE userdomain
(
	usrDomainId   int            unsigned primary key NOT NULL auto_increment COMMENT '����ID',
	usrName       char(64)       NOT NULL COMMENT '�û���',		
	domainIdSet   varchar(1024)  NOT NULL COMMENT '��ID���ϣ���1,2,3,4,5',		
	des           varchar(256)   NULL default 'des' COMMENT '����'
)
ENGINE=InnoDB DEFAULT CHARSET=utf8;



/* === (9) device === */
SELECT '-----device prcess--------';

CREATE TABLE device 
(  
	id             int                   unsigned     primary key NOT NULL auto_increment COMMENT '����ID',
	name           char(64)  	         NOT NULL  COMMENT '�豸����',
	sn             char(32)  	         NULL default '' COMMENT 'SN��GSM��������豸����û�и��ֶ�',
	carrier        tinyint               NULL COMMENT '�����ڱ�ʶGSM���ز���0���ز�0��1���ز�1',
	bindingDevId   int                   unsigned     NULL COMMENT '�����ڱ�ʶGSM�İ��豸id', 
	ipAddr         char(32)  	         NULL COMMENT 'IP��ַ',
	port           smallint              unsigned  NULL COMMENT '�˿ں�',
	netmask        char(16)  	         NULL COMMENT '����',
	mode           char(16)              NULL COMMENT '�豸��ʽ��LTE-TDD��LTE-FDD��GSM��WCDMA��', 
	online         tinyint      	     NULL default 0  COMMENT '�����߱�ʶ��0�����ߣ�1������',
	lastOnline     datetime              NULL COMMENT '��������ʱ��',
	isActive       tinyint      	     NULL default 1  COMMENT '��ʶ���豸�Ƿ���Ч��0����Ч��1����Ч',
	affDomainId    int              	 NOT NULL COMMENT '��ʶ�豸�Ĵ������Ǹ���FK'
 
) ENGINE=InnoDB DEFAULT  CHARSET=utf8; 




/* === (10) bwlist === */
SELECT '-----bwlist prcess--------';

CREATE TABLE bwlist 
(  
	id             int      unsigned     primary key NOT NULL auto_increment COMMENT '����ID',
	imsi           char(15)  	         NULL  COMMENT 'IMSI��',
	imei           char(15)  	         NULL  COMMENT 'IMEI��',
	bwFlag         enum('black','white','other') DEFAULT 'other' COMMENT '�������ͱ�ʶ',
	rbStart        tinyint      	     NULL default 0  COMMENT '��ʼRB',
	rbEnd          tinyint      	     NULL default 0  COMMENT '����RB',
	time           datetime              NULL COMMENT '����ʱ��',
	des            varchar(128)  	     NULL default '' COMMENT '����',
	linkFlag       tinyint      	     NOT NULL default 0  COMMENT '0�����ӵ�DeviceId,1�����ӵ�DomainId',
	affDeviceId    int                   NULL COMMENT '�������豸ID�ţ���������device��',
	affDomainId    int              	 NULL COMMENT '��ʶ�豸�Ĵ������Ǹ���FK' 
	
) ENGINE=InnoDB DEFAULT  CHARSET=utf8;