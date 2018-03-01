# 轻量级ORM工具

标签（空格分隔）： .NET Develop

---

[Example](#example)
[Sql Mapper](#sqlmapper)


## 简介
一个轻量级ORM工具，基于Dapper.Net

## Init
```
DbContext.SetConfig("YourSqlConnectionString", TimeOut);
```

## Attributes

### TableNameAttribute
表示类的表名，不设置则默认为类名

### SqlIgnoreAttribute
生成的SQL语句将忽略这一个属性

### SinceTheIncreaseAttribute
表示自增列，Insert以及Update时将忽略，不修改


<span id="example"></span>
## Examples
### Example Class
```
class Customer{
    [Key] //标记为Key特性，或者ID的，表示为主键
    public int? ID { get; set; }
    public string Name { get; set; }
    public int? Age { get; set; }
}
```

### Insert
```
var model = new Customer()
{
    Name = "Yours Name",
    Age = 18
};

model.Insert();
```

### Update
```
var model = new Customer()
{
    Name = "Yours Name Update",
    ID = 1
};

model.Update(false); //更新ID为1的属性不为null的实体，此处只更新Name值

model.Update(); //更新所有属性

```
### Delete
```
var model = new Customer()
{
    ID = 1
};

model.Delete(); //删除ID=1的
```
### Query
```
var model = new Customer()
{
    Age = 18
};

var collection = model.Query<Customer>(); //返回Age=18的结果集

var model2 = new Customer()
{
    ID = 1
};

var result = model2.Get<Customer>(); //返回ID=1的结果

```

<span id="sqlmapper"></span>
## Sql Mapper
### Create Xml
```
<?xml version="1.0" encoding="utf-8" ?>
<mapper xsi:noNamespaceSchemaLocation="../DTD/SqlMapper.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <select id="testsql">
    select name, age from user
    <trim prefix="where" prefixOverrides="and">
      <if test="args.name.length === 10">
        and name = @name
      </if>
      <if test="args.name.length !== 10">
        and id = @name
      </if>
      <if test="args.keyword !== null">
        and name like '%' + @keyword + '%'
      </if>
      and status = 1
    </trim>
  </select>
</mapper>
```

### Generate Sql Code
```
SqlMapper.Init(@"Your WWWroot", "MySqlMapper.xml");

var sql = SqlMapper.Get("testsql", new { name = "123456789", keyword = (string)null });

// select name, age from user where id = @name and status = 1
Console.WriteLine(sql);

```

### Using
```
var collections = new BaseModel()<Customer>("sqlMapperId", new { Name = "name" });
```
