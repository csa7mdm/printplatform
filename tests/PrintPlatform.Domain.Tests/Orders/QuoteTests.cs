using System;
using FluentAssertions;
using PrintPlatform.Domain.Orders;
using Xunit;

namespace PrintPlatform.Domain.Tests.Orders;

public class QuoteTests
{
    private static Quote CreateSut()
    {
        return Quote.CreateDraft(Guid.NewGuid());
    }

    [Fact]
    public void SetPricing_WithValidValues_AssignsPropertiesAndCalculatesTotalsCorrectly()
    {
        // Arrange
        var quote = CreateSut();
        decimal estimatedWeightGrams = 100m;
        decimal estimatedPrintHours = 5.5m;
        decimal materialCostEgp = 50m;
        decimal depreciationCostEgp = 10m;
        decimal laborCostEgp = 20m;
        decimal packagingCostEgp = 5m;
        decimal failureAllowanceEgp = 15m;
        decimal platformMarginEgp = 25m;
        decimal designServicePriceEgp = 30m;
        decimal deliveryEstimateEgp = 40m;
        DateTimeOffset estimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7);
        DateTimeOffset validUntil = DateTimeOffset.UtcNow.AddDays(14);
        string operatorNotes = "Looks good";

        // Act
        quote.SetPricing(
            estimatedWeightGrams,
            estimatedPrintHours,
            materialCostEgp,
            depreciationCostEgp,
            laborCostEgp,
            packagingCostEgp,
            failureAllowanceEgp,
            platformMarginEgp,
            designServicePriceEgp,
            deliveryEstimateEgp,
            estimatedDeliveryDate,
            validUntil,
            operatorNotes);

        // Assert
        quote.EstimatedWeightGrams.Should().Be(estimatedWeightGrams);
        quote.EstimatedPrintHours.Should().Be(estimatedPrintHours);
        quote.MaterialCostEgp.Should().Be(materialCostEgp);
        quote.DepreciationCostEgp.Should().Be(depreciationCostEgp);
        quote.LaborCostEgp.Should().Be(laborCostEgp);
        quote.PackagingCostEgp.Should().Be(packagingCostEgp);
        quote.FailureAllowanceEgp.Should().Be(failureAllowanceEgp);
        quote.PlatformMarginEgp.Should().Be(platformMarginEgp);
        quote.DesignServicePriceEgp.Should().Be(designServicePriceEgp);
        quote.DeliveryEstimateEgp.Should().Be(deliveryEstimateEgp);
        quote.EstimatedDeliveryDate.Should().Be(estimatedDeliveryDate);
        quote.ValidUntil.Should().Be(validUntil);
        quote.OperatorNotes.Should().Be(operatorNotes);

        decimal expectedFulfillmentCost = materialCostEgp + depreciationCostEgp + laborCostEgp + packagingCostEgp + failureAllowanceEgp;
        quote.TotalFulfillmentCostEgp.Should().Be(expectedFulfillmentCost);

        decimal expectedCustomerPrice = expectedFulfillmentCost + platformMarginEgp;
        quote.CustomerPriceEgp.Should().Be(expectedCustomerPrice);

        decimal expectedTotalPrice = expectedCustomerPrice + designServicePriceEgp + deliveryEstimateEgp;
        quote.TotalPriceEgp.Should().Be(expectedTotalPrice);
    }

    [Fact]
    public void SetPricing_WithZeroValues_CalculatesCorrectly()
    {
        // Arrange
        var quote = CreateSut();
        decimal estimatedWeightGrams = 0m;
        decimal estimatedPrintHours = 0m;
        decimal materialCostEgp = 0m;
        decimal depreciationCostEgp = 0m;
        decimal laborCostEgp = 0m;
        decimal packagingCostEgp = 0m;
        decimal failureAllowanceEgp = 0m;
        decimal platformMarginEgp = 0m;
        decimal designServicePriceEgp = 0m;
        decimal deliveryEstimateEgp = 0m;
        DateTimeOffset estimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7);
        DateTimeOffset validUntil = DateTimeOffset.UtcNow.AddDays(14);

        // Act
        quote.SetPricing(
            estimatedWeightGrams,
            estimatedPrintHours,
            materialCostEgp,
            depreciationCostEgp,
            laborCostEgp,
            packagingCostEgp,
            failureAllowanceEgp,
            platformMarginEgp,
            designServicePriceEgp,
            deliveryEstimateEgp,
            estimatedDeliveryDate,
            validUntil,
            null);

        // Assert
        quote.EstimatedWeightGrams.Should().Be(0m);
        quote.EstimatedPrintHours.Should().Be(0m);
        quote.MaterialCostEgp.Should().Be(0m);
        quote.DepreciationCostEgp.Should().Be(0m);
        quote.LaborCostEgp.Should().Be(0m);
        quote.PackagingCostEgp.Should().Be(0m);
        quote.FailureAllowanceEgp.Should().Be(0m);
        quote.PlatformMarginEgp.Should().Be(0m);
        quote.DesignServicePriceEgp.Should().Be(0m);
        quote.DeliveryEstimateEgp.Should().Be(0m);
        quote.TotalFulfillmentCostEgp.Should().Be(0m);
        quote.CustomerPriceEgp.Should().Be(0m);
        quote.TotalPriceEgp.Should().Be(0m);
    }

    [Theory]
    [InlineData(10.123, 20.456, 30.789, 40.111, 50.222, 60.333, 70.444, 80.555)]
    [InlineData(0.001, 0.002, 0.003, 0.004, 0.005, 0.006, 0.007, 0.008)]
    public void SetPricing_WithFractionalValues_MaintainsPrecision(
        decimal materialCostEgp,
        decimal depreciationCostEgp,
        decimal laborCostEgp,
        decimal packagingCostEgp,
        decimal failureAllowanceEgp,
        decimal platformMarginEgp,
        decimal designServicePriceEgp,
        decimal deliveryEstimateEgp)
    {
        // Arrange
        var quote = CreateSut();

        // Act
        quote.SetPricing(
            1m,
            1m,
            materialCostEgp,
            depreciationCostEgp,
            laborCostEgp,
            packagingCostEgp,
            failureAllowanceEgp,
            platformMarginEgp,
            designServicePriceEgp,
            deliveryEstimateEgp,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null);

        // Assert
        decimal expectedFulfillmentCost = materialCostEgp + depreciationCostEgp + laborCostEgp + packagingCostEgp + failureAllowanceEgp;
        quote.TotalFulfillmentCostEgp.Should().Be(expectedFulfillmentCost);

        decimal expectedCustomerPrice = expectedFulfillmentCost + platformMarginEgp;
        quote.CustomerPriceEgp.Should().Be(expectedCustomerPrice);

        decimal expectedTotalPrice = expectedCustomerPrice + designServicePriceEgp + deliveryEstimateEgp;
        quote.TotalPriceEgp.Should().Be(expectedTotalPrice);
    }

    [Fact]
    public void SetPricing_CalledMultipleTimes_UpdatesValuesCorrectly()
    {
        // Arrange
        var quote = CreateSut();

        // First Call
        quote.SetPricing(1m, 1m, 10m, 10m, 10m, 10m, 10m, 10m, 10m, 10m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);
        quote.TotalPriceEgp.Should().Be(80m); // (10+10+10+10+10) + 10 + 10 + 10

        // Act - Second Call
        quote.SetPricing(2m, 2m, 20m, 20m, 20m, 20m, 20m, 20m, 20m, 20m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);

        // Assert
        quote.EstimatedWeightGrams.Should().Be(2m);
        quote.EstimatedPrintHours.Should().Be(2m);
        quote.MaterialCostEgp.Should().Be(20m);

        decimal expectedFulfillmentCost = 20m * 5; // 100
        quote.TotalFulfillmentCostEgp.Should().Be(expectedFulfillmentCost);

        decimal expectedCustomerPrice = expectedFulfillmentCost + 20m; // 120
        quote.CustomerPriceEgp.Should().Be(expectedCustomerPrice);

        decimal expectedTotalPrice = expectedCustomerPrice + 20m + 20m; // 160
        quote.TotalPriceEgp.Should().Be(expectedTotalPrice);
    }
}
