# 轻量级ORM工具

标签（空格分隔）： .NET Develop

---

## 说明
一个轻量级ORM工具，基于Dapper.Net
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
