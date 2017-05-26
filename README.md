# SharpSoft.Data
通用数据库操作类库，实现一种统一的基础SQL语法，可以翻译到不同类型的数据库中执行，从而无需为不同的数据库书写不同的操作指令。

//HOW TO USE?

            string sql =
                @"
if 1>2{
select switch(a){1:'aaa',2:'bbb',default:null}
}
else if(2>3)
select  switch(b){1:111,2:222,default:555}
else
select  switch(b){1:111,2:222,default:555}

declare @wwww varchar(12);
select a.* ,a.dd,a,ff,12 as ddd,b.* as d from a,(select * from c) as b
join c on a.f1==c.f1
left join d on d.f1==c.f1
full join (select * from table1) as f on a.f2==f.f2
where a.id==1 and exists(select * from gg)
group by a.id,a.id1,a.id2
having b.ggggg==d.ggg
order by a.id asc,b.id desc
union all
select * from xxxxx limit 2,10
delete t1 from t1,t2 
join s on s.f1==t1.f1
where t1.f2='xxxxx'
INSERT INTO TABLE1(F1,F2,F3)VALUES(12,45,'333')
INSERT INTO TABLE2 VALUES(1,2,3,4,'5')
UPDATE TABLE3 SET F1=@PARA,F2=12*45+45*MAX(1,2,3,4,5)
WHERE TABLE3.ID==2
";
            TSQLParser SP = new TSQLParser(sql); 
            List<IStatement> l = SP.ReadStatements();//得到解析后的语法树
            MsSql ms = new MsSql("Data Source=.;User ID=sa;Password=123456;");
            string tsql = ms.Setting().Generate(l.ToArray());//将语法树翻译到对应的目标数据库，得到适用的SQL指令
