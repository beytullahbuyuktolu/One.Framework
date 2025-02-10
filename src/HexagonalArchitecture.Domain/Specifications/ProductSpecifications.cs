using HexagonalArchitecture.Domain.Entities;

namespace HexagonalArchitecture.Domain.Specifications;

public class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(Guid id)
    {
        Criteria = x => x.Id == id;
    }
}

public class ProductByCodeSpecification : BaseSpecification<Product>
{
    public ProductByCodeSpecification(string code)
    {
        Criteria = x => x.Code == code;
    }
}

public class ProductByNameSpecification : BaseSpecification<Product>
{
    public ProductByNameSpecification(string name)
    {
        Criteria = x => x.Name == name;
    }
}

public class ProductByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        Criteria = x => x.Price >= minPrice && x.Price <= maxPrice;
    }
}
