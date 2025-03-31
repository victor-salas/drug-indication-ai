# 💊 Drug Indication Processing API

This project is a full-stack .NET 8 microservice built to extract, normalize, enrich, and expose structured drug-indication program data, using both rule-based logic and generative AI.

It was created as part of a technical challenge. The goal is to transform messy input (e.g., copay program JSON, free-text eligibility details) into rich, queryable data — using ICD-10 mappings, eligibility extraction, and data standardization.

## 📂 Project Structure

| Project                         | Description |
|--------------------------------|-------------|
| `DrugIndication.API`           | ASP.NET Core Web API exposing RESTful endpoints |
| `DrugIndication.Domain`        | Domain models, entities, and config classes 
| `DrugIndication.Parsing`       | Data transformation layer using AI, ICD-10 mapping, enrichment |
| `DrugIndication.Application`   | Authentication service |
| `DrugIndication.Infrastructure`| MongoDB persistence layer |
| `DrugIndication.Tests`         | Unit tests using xUnit, Moq, and FluentAssertions |

## 🌐 API Overview

### 🔐 Authentication

- **JWT-based authentication** with role-based authorization.
- Roles: `admin`, `user`
- Endpoints are protected using `[Authorize]` or `[Authorize(Roles = "admin")]`.

### 📌 Endpoints

| Endpoint | Method | Description | Auth |
|---------|--------|-------------|------|
| `/auth/register` | POST | Register new user (`username`, `password`, `role`) | ❌ |
| `/auth/login`    | POST | Get JWT token | ❌ |
| `/programs`      | GET  | Get all programs | ✅ |
| `/programs/{id}` | GET  | Get single program by ID | ✅ |
| `/programs`      | POST | Add a new program (raw input JSON) | ✅ `admin` |
| `/programs/{id}` | PUT  | Update a program | ✅ `admin` |
| `/programs/{id}` | DELETE | Delete a program | ✅ `admin` |

## 🧠 Core Logic

### 🧬 1. Indication Mapping (ICD-10)

We map free-text like `"asthma"` to the best ICD-10 code using:

- **Primary**: OpenAI (`GPT-3.5-turbo`)
- **Fallback**: local CSV dataset + fuzzy matching

### 🧾 2. Eligibility Extraction

Free-text eligibility (e.g., `"Must be 18+"`) is transformed into:

```json
[{"name": "minimum_age", "value": "18"}]
```

### 💵 3. Benefit Extraction

AI scans fields like `ProgramDetails`, `AnnualMax`, `CouponVehicle`, etc., and extracts structured benefits.

### 🗓 4. Expiration Date Normalization

OpenAI transforms text like `"end of year"` into `2025-12-31`.

### 🧩 5. Detailed Program Info

We extract structured info like:

```json
[{ "program": "...$0 copay..." }, { "renewal": "every Jan 1st" }]
```

## 🗃 Example Output

```json
{
  "id": "67eaa3307f4b4aeacca94b62",
  "programID": 11757,
  "programName": "Dupixent MyWay Copay Card",
  "drugs": [
    "Dupixent",
    "Dupixent Pen"
  ],
  "programType": null,
  "requirements": [
    {
      "name": "us_residency",
      "value": "true"
    },
    {
      "name": "insurance_coverage",
      "value": "true"
    }
  ],
  "benefits": [
    {
      "name": "max_annual_savings",
      "value": "13000.00"
    },
    {
      "name": "income_requirements",
      "value": "none"
    },
    {
      "name": "valid_pharmacies",
      "value": "participating"
    },
    {
      "name": "activation_required",
      "value": "no"
    }
  ],
  "forms": null,
  "funding": null,
  "details": [
    {
      "eligibility": "No income requirements"
    },
    {
      "program": "Covers up to $13,000 annually in copay assistance"
    },
    {
      "renewal": "Valid at participating pharmacies only"
    }
  ],
  "programUrl": "https://www.dupixent.com/myway",
  "indications": [
    {
      "description": "Moderate-to-severe atopic dermatitis",
      "icd10Code": "L20.9",
      "icd10Description": "Atopic dermatitis, unspecified",
      "mappingSource": "ai"
    },
    {
      "description": "Asthma",
      "icd10Code": "J45.909",
      "icd10Description": "Asthma, unspecified, uncomplicated",
      "mappingSource": "ai"
    },
    {
      "description": "Eosinophilic esophagitis",
      "icd10Code": null,
      "icd10Description": null,
      "mappingSource": "unmappable"
    }
  ],
  "associatedFoundations": [
    {
      "programID": 30683,
      "programName": "Accessia Health: Alpha-1 Antitrypsin Deficiency - Private Insurance: Waitlist",
      "foundationFundLevels": [
        "Empty"
      ],
      "therapAreas": [
        "Alpha-1 Antitrypsin Deficiency"
      ],
      "drugs": [
        "Glassia",
        "Prolastin-C",
        "Zemaira",
        "Aralast NP"
      ],
      "indications": [
        {
          "description": "Alpha-1 Antitrypsin Deficiency",
          "icd10Code": "E88.0",
          "icd10Description": "Alpha-1-antitrypsin deficiency",
          "mappingSource": "ai"
        }
      ]
    },
    {
      "programID": 30778,
      "programName": "Accessia Health: Asthma - Public Insurance: Waitlist",
      "foundationFundLevels": [
        "Empty"
      ],
      "therapAreas": [
        "Asthma",
        "Severe Asthma"
      ],
      "drugs": [
        "Fasenra",
        "Xolair",
        "Dupixent"
      ],
      "indications": [
        {
          "description": "Asthma",
          "icd10Code": "J45.909",
          "icd10Description": "Asthma, unspecified, uncomplicated",
          "mappingSource": "ai"
        },
        {
          "description": "Severe Asthma",
          "icd10Code": "J45.50",
          "icd10Description": "Severe persistent asthma, uncomplicated",
          "mappingSource": "ai"
        }
      ]
    }
  ],
  "expirationDate": null
}
```

## 🐳 Dockerized Deployment

```bash
docker-compose up --build
```

Visit: [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

## 🔒 Authentication Flow

1. `POST /auth/register`
2. `POST /auth/login`
3. Copy JWT → Use **Authorize** in Swagger: `eyJksjd83jfhfhf...` (without word 'Bearer', you only need to paste the token)

## 🧪 Testing

```bash
dotnet test
```

## ⚙️ Configuration

```json
"OpenAI": { "ApiKey": "your-api-key" },
"Jwt": { "Key": "...", "Issuer": "DrugApi", "Audience": "DrugUsers" },
"Mongo": { "ConnectionString": "mongodb://mongo:27017", "Database": "DrugDb" }
```

## ⚙️ Execution

- Download the repository.
- Run the project with: docker-compose up --build
- Go to http://localhost:8080/swagger/index.html

## 📈 Scalability

- NoSQL dynamic schema
- AI fallback
- Decoupled architecture
- Retry/backoff ready

## 🚧 Improvements

- Add pagination
- Add refresh tokens
- Add timestamps
- More test coverage
- Retry logic for OpenAI

## ✅ Conclusion

This project demonstrates:
- Clean architecture separation
- Effective use of Generative AI for data enrichment
- Secure, scalable, testable API in .NET 8
