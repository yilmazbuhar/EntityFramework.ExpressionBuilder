# Expression Filters and Sorts

### Filtering Database

```
var filteredcustomers = await customerdb.Customer
    .ApplyFilterAndSort(jsonFilter, null);
```

## Roadmap
* Subtypes for filter
* Subtypes for sort
* Multiple sort field
