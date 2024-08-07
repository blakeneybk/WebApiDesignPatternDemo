This is a demonstration of what a take home assessment might look like if it were treated like the core of a production funding calculation service that is anticipated to see a lot of extension, changes, and evolution.

## Design Patterns

### Strategy

The strategy pattern provides a flexible way to create and switch between different calculations and validations without altering existing functionality. It facilitates easy modification of behavior during runtime if necessary.

### Chain of Responsibility

As we deal with step-by-step processes, having a customizable set of steps leading to a final result is crucial. The Chain of Responsibility (CoR) pattern offers great flexibility for this purpose. Each handler in the chain has a single responsibility, employing various strategies. Different pathways can be chained off, with each link being atomic and only engaged if the context meets its requirements.

### Repository

The Repository pattern is a classic approach that simplifies the swapping of data persistence architecture. It provides a straightforward way to manage data access logic separately from business logic.

### Factory

Factories play a critical role in making the strategy pattern flexible. They enable the creation of strategy instances on the fly, making it easy to swap out implementations of strategies as needed.

### Dependency Injection

At the heart of the application is the .NET Dependency Injection (DI) system. Having everything under DI control simplifies the swapping of implementations and provides a clear structure for the application.

### Rate Limiting

The SEC Edgar API imposes rate limits, which we've addressed by implementing a rate limiter using a Semaphore and a Timer. This ensures that we adhere to the API's rate limits and prevent excessive requests.

----------------------------------------------------------------------------------------------

## Step 1: Using the SEC’s EDGAR API
Import and persist data for the CIKs (Central Index Key) listed at the bottom of this document.

**API Endpoint:** `https://data.sec.gov/api/xbrl/companyfacts/CIK##########.json`  
where `##########` is the entity’s 10-digit Central Index Key (CIK), including leading zeros.

**Headers (to avoid authorization errors):**
- User-Agent: `PostmanRuntime/7.34.0`
- Accept: `*/*`

**Example API Response:**
```json
{
  "cik": 1543151,
  "entityName": "UBER TECHNOLOGIES, INC.",
  "facts": {
    "us-gaap": {
      "NetIncomeLoss": {
        "units": {
          "USD": [
            {
              "start": "2017-01-01",
              "end": "2017-12-31",
              "val": -4033000000,
              "accn": "0001543151-20-000010",
              "fy": 2019,
              "fp": "FY",
              "form": "10-K",
              "filed": "2020-03-02",
              "frame": "CY2017"
            }
          ]
        }
      }
    }
  }
}
```

**Focus on extracting:** `cik`, `entityName`, and the `NetIncomeLoss > units > USD` array – specifically the `val`, `form`, and `frame` fields.

## Step 2: API Endpoint for Funding Eligibility
We’d like to offer an HTTP-based API endpoint for retrieving a list of companies as well as the amount of funding they are eligible to receive. The request should optionally allow the user to supply a parameter that can be used to return only companies where their name starts with the specified letter. The response payload for the endpoint should be in the following format (this is important since we will use the output to test for correct output!):
```json
[
  {
    "id": 1,
    "name": "Uber, Inc.",
    "standardFundableAmount": 123.45,
    "specialFundableAmount": 234.56
  }
]
```

### Calculation Rules
**How to calculate Standard Fundable Amount:**
- Company must have income data for all years between (and including) 2018 and 2022. If they did not, their Standard Fundable Amount is $0.
- Company must have had positive income in both 2021 and 2022. If they did not, their Standard Fundable Amount is $0.
- Using the highest income between 2018 and 2022:
  - If income is greater than or equal to $10B, standard fundable amount is 12.33% of income.
  - If income is less than $10B, standard fundable amount is 21.51% of income.

**How to calculate the Special Fundable Amount:**
- Initially, the Special Fundable Amount is the same as the Standard Fundable Amount.
- If the company name starts with a vowel, add 15% to the standard funding amount.
- If the company’s 2022 income was less than their 2021 income, subtract 25% from their standard funding amount

## CIKs
18926,892553,1510524,1858912,1828248,1819493,60086,1853630,1761312,1851182,1034665,927628,1125259,1547660,1393311,1757143,1958217,312070,310522,1861841,1037868,1696355,1166834,915912,1085277,831259,882291,1521036,1824502,1015647,884624,1501103,1397183,1552797,1894630,823277,21175,1439124,52827,1730773,1867287,1685428,1007587,92103,1641751,6845,1231457,947263,895421,1988979,1848898,844790,1541309,1858007,1729944,726958,1691221,730272,1308106,884144,1108134,1849058,1435617,1857518,64803,1912498,1447380,1232384,1141788,1549922,914475,1498382,1400897,314808,1323885,1526520,1550695,1634293,1756708,1540159,1076691,1980088,1532346,923796,1849635,1872292,1227857,1046311,1710350,1476150,1844642,1967078,14272,933267,1157557,1560293,217410,1798562,1038074,1843370

## EdgarCompanyInfo.cs

```csharp
using System.Text.Json.Serialization;

public class EdgarCompanyInfo
{
    public int Cik { get; set; }
    public string EntityName { get; set; }
    public InfoFact Facts { get; set; }

    public class InfoFact
    {
        [JsonPropertyName("us-gaap")]
        public InfoFactUsGaap UsGaap { get; set; }
    }

    public class InfoFactUsGaap
    {
        public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; }
    }

    public class InfoFactUsGaapNetIncomeLoss
    {
        public InfoFactUsGaapIncomeLossUnits Units { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnits
    {
        public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnitsUsd
    {
        /// <summary>
        /// Possibilities include 10-Q, 10-K,8-K, 20-F, 40-F, 6-K, and
        /// their variants. YOU ARE INTERESTED ONLY IN 10-K DATA!
        /// </summary>
        public string Form { get; set; }

        /// <summary>
        /// For yearly information, the format is CY followed by the year
        /// number. For example: CY2021. YOU ARE INTERESTED ONLY IN YEARLY INFORMATION
        /// WHICH FOLLOWS THIS FORMAT!
        /// </summary>
        public string Frame { get; set; }

        /// <summary>
        /// The income/loss amount.
        /// </summary>
        public decimal Val { get; set; }
    }
}
```
