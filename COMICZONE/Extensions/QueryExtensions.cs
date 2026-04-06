using System.Linq.Expressions;
using System.Reflection;

namespace COMICZONE.Extensions
{
    public static class QueryExtensions
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? keyword, params string[] properties)
        {
            if (string.IsNullOrWhiteSpace(keyword) || properties == null || properties.Length == 0)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            foreach (var propertyName in properties)
            {
                var property = GetProperty(typeof(T), propertyName);
                if (property == null) continue;

                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                
                // Convert to string and Call .Contains()
                var toStringMethod = typeof(object).GetMethod("ToString");
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                if (toStringMethod == null || containsMethod == null) continue;

                var stringExpression = property.PropertyType == typeof(string) 
                    ? (Expression)propertyAccess 
                    : Expression.Call(propertyAccess, toStringMethod);

                var keywordExpression = Expression.Constant(keyword);
                var containsExpression = Expression.Call(stringExpression, containsMethod, keywordExpression);

                combinedExpression = combinedExpression == null 
                    ? containsExpression 
                    : Expression.OrElse(combinedExpression, containsExpression);
            }

            if (combinedExpression == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortColumn, bool isAscending)
        {
            if (string.IsNullOrWhiteSpace(sortColumn))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? propertyAccess = null;

            try
            {
                if (sortColumn.Equals("Artists", StringComparison.OrdinalIgnoreCase))
                {
                    // Special case for Artists collection: sort by the first artist's name
                    var artistsProp = typeof(T).GetProperty("Artists");
                    if (artistsProp == null) return query;

                    var artistsAccess = Expression.MakeMemberAccess(parameter, artistsProp);
                    var artistType = artistsProp.PropertyType.GetGenericArguments().FirstOrDefault() 
                                     ?? artistsProp.PropertyType.GetInterfaces()
                                         .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                         ?.GetGenericArguments().FirstOrDefault();

                    if (artistType == null) return query;

                    var nameProp = artistType.GetProperty("Name");
                    if (nameProp == null) return query;

                    // Build: x.Artists.Select(a => a.Name).FirstOrDefault()
                    var selectMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(artistType, typeof(string));

                    var firstOrDefaultMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1)
                        .MakeGenericMethod(typeof(string));

                    var artistParam = Expression.Parameter(artistType, "a");
                    var nameAccess = Expression.MakeMemberAccess(artistParam, nameProp);
                    var selectLambda = Expression.Lambda(nameAccess, artistParam);

                    var selectCall = Expression.Call(selectMethod, artistsAccess, selectLambda);
                    propertyAccess = Expression.Call(firstOrDefaultMethod, selectCall);
                }
                else if (sortColumn.EndsWith(".Count", StringComparison.OrdinalIgnoreCase))
                {
                    // Sort by the count of a collection (e.g., Products.Count)
                    var collectionName = sortColumn.Substring(0, sortColumn.Length - 6);
                    var collectionProp = typeof(T).GetProperty(collectionName);
                    if (collectionProp == null) return query;

                    var collectionAccess = Expression.MakeMemberAccess(parameter, collectionProp);
                    var countProp = collectionProp.PropertyType.GetProperty("Count");

                    if (countProp != null)
                    {
                        propertyAccess = Expression.MakeMemberAccess(collectionAccess, countProp);
                    }
                    else
                    {
                        // Fallback to Enumerable.Count() extension method
                        var elementType = collectionProp.PropertyType.GetGenericArguments().FirstOrDefault()
                                         ?? collectionProp.PropertyType.GetInterfaces()
                                             .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                             ?.GetGenericArguments().FirstOrDefault();

                        if (elementType == null) return query;

                        var countMethod = typeof(Enumerable).GetMethods()
                            .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                            .MakeGenericMethod(elementType);

                        propertyAccess = Expression.Call(countMethod, collectionAccess);
                    }
                }
                else if (sortColumn.Equals("TotalValue", StringComparison.OrdinalIgnoreCase))
                {
                    // Special case for TotalValue calculation (usually for Carts/Orders)
                    // Assuming the model has a collection called "CartItems" or "OrderItems"
                    var itemsProp = typeof(T).GetProperty("CartItems") ?? typeof(T).GetProperty("OrderItems");
                    if (itemsProp == null) return query;

                    var itemsAccess = Expression.MakeMemberAccess(parameter, itemsProp);
                    var itemType = itemsProp.PropertyType.GetGenericArguments().FirstOrDefault()
                                   ?? itemsProp.PropertyType.GetInterfaces()
                                       .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                       ?.GetGenericArguments().FirstOrDefault();

                    if (itemType == null) return query;

                    var quantityProp = itemType.GetProperty("Quantity");
                    var productProp = itemType.GetProperty("Product");
                    if (quantityProp == null || productProp == null) return query;

                    var productType = productProp.PropertyType;
                    var priceProp = productType.GetProperty("Price");
                    if (priceProp == null) return query;

                    // Build lambda: i => i.Quantity * (i.Product.Price ?? 0)
                    var itemParam = Expression.Parameter(itemType, "i");
                    var quantityAccess = Expression.MakeMemberAccess(itemParam, quantityProp);
                    var productAccess = Expression.MakeMemberAccess(itemParam, productProp);
                    var priceAccess = Expression.MakeMemberAccess(productAccess, priceProp);

                    // Sum expects i.Quantity * i.Product.Price to return an int (or decimal/long)
                    Expression priceValue = priceAccess;
                    if (priceProp.PropertyType == typeof(int?) || Nullable.GetUnderlyingType(priceProp.PropertyType) != null)
                    {
                        priceValue = Expression.Coalesce(priceAccess, Expression.Constant(0));
                    }
                    
                    var multiply = Expression.Multiply(quantityAccess, Expression.Convert(priceValue, typeof(int)));
                    var sumLambda = Expression.Lambda(multiply, itemParam);

                    var sumMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "Sum" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType.GetGenericArguments()[1] == typeof(int))
                        .MakeGenericMethod(itemType);

                    propertyAccess = Expression.Call(sumMethod, itemsAccess, sumLambda);
                }
                else
                {


                    // Support nested properties like ProductReviewSummary.Averagerating
                    propertyAccess = parameter;
                    foreach (var part in sortColumn.Split('.'))
                    {
                        var property = GetProperty(propertyAccess.Type, part);
                        if (property == null) return query;
                        propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                    }
                }
            }
            catch
            {
                return query;
            }

            if (propertyAccess == null) return query;

            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = isAscending ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(typeof(Queryable), methodName,
                new Type[] { typeof(T), propertyAccess.Type },
                query.Expression, Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }


        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private static PropertyInfo? GetProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }
    }
}
