# Expression Filters and Sorts

Expression builder is a simple library that allows you to perform generic filtering, paging and sorting on your Entity framework queries.

**Json Filter Example**

```
"Where": [
    {
      "Member": "Name",
      "Operator": "contains",
      "Value": "s",
      "LogicalOperator": "or",
      "Active": false
    }
  ],
  "Sort": [
    {
      "Field": "Name",
      "Direction": "ASC"
    },
    {
      "Field": "Surname",
      "Direction": "DESC"
    }
  ]
}
```

**Apply filter and sort**

```
var filteredcustomers = await customerdb.Customer
    .ApplyFilterAndSort(jsonFilter, null);
```

## Roadmap
* ~~Subtypes for filter~~
* ~~Subtypes for sort~~
* ~~Multiple sort field~~
* Paging
* Performance tunning