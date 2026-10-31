// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Offering.Declaration.for_ProductOfferingBuilder.given;

/// <summary>
/// A <see cref="ProductOfferingBuilder"/> that has a product, to declare an offering on.
/// </summary>
public class an_offering_being_declared : Specification
{
    protected ProductOfferingBuilder _builder;
    protected ProductOffering _offering;
    protected InvalidOffering _invalid;

    void Establish() => _builder = new ProductOfferingBuilder().Product(ProductId.New());

    /// <summary>
    /// Builds the offering, keeping what was thrown.
    /// </summary>
    protected void Build()
    {
        try
        {
            _offering = _builder.Build();
        }
        catch (InvalidOffering invalid)
        {
            _invalid = invalid;
        }
    }
}
#endif
