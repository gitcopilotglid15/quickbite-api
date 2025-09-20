# Food Menu Management API - Project Requirement Document

## Overview

QuickBite is a RESTful API system for managing restaurant food menus, enabling restaurants to create, update, and organize their menu items with AI-powered search capabilities. The system provides CRUD operations for menu management and intelligent search functionality to help customers discover menu items based on dietary preferences, ingredients, and descriptions.

## Goals

- **Primary**: Build a robust API for menu item management with full CRUD operations
- **Secondary**: Implement AI-powered search to enhance menu discovery experience
- **Tertiary**: Provide a scalable foundation for future restaurant management features
- **Learning**: Demonstrate modern .NET development practices with TDD and clean architecture

## Non-Goals

- Frontend web application (API-only)
- User authentication/authorization system
- Payment processing integration
- Inventory management
- Multi-restaurant/tenant support (single restaurant focus)
- Real-time notifications

## Target Users

- **Primary**: Backend developers learning API development
- **Secondary**: Restaurant owners (future frontend consumers)
- **Tertiary**: Training participants practicing .NET development

## MVP Feature List

### 1. Menu Item Management
- Create new menu items with details
- Retrieve menu items (single and list)
- Update existing menu items
- Delete menu items
- Category-based filtering

### 2. AI-Powered Search
- Search menu items by natural language queries
- Filter by dietary restrictions (vegetarian, vegan, gluten-free)
- Ingredient-based search
- Fuzzy matching for typos and variations

### 3. Data Validation & Error Handling
- Input validation for all endpoints
- Standardized error responses
- Request/response logging

## Acceptance Criteria

### Menu Item Management
- ✅ API can create menu items with name, description, price, category, ingredients, and dietary tags
- ✅ API returns single menu item by ID with 200 status or 404 if not found
- ✅ API returns paginated list of menu items with filtering options
- ✅ API updates menu items and returns updated data with 200 status
- ✅ API deletes menu items and returns 204 status
- ✅ API validates required fields and returns 400 for invalid requests
- ✅ API supports filtering by category (appetizers, mains, desserts, beverages)

### AI Search Feature
- ✅ API accepts natural language search queries (e.g., "spicy chicken dishes")
- ✅ API returns relevant menu items ranked by relevance score
- ✅ API supports dietary filter queries (e.g., "vegan pasta options")
- ✅ API handles ingredient-based searches (e.g., "contains mushrooms")
- ✅ API provides fuzzy matching for misspelled ingredient names
- ✅ Search returns empty array for no matches with 200 status

### Error Handling & Validation
- ✅ API returns standardized error format with error code and message
- ✅ API validates price as positive decimal number
- ✅ API validates required fields (name, price, category)
- ✅ API logs all requests and responses for debugging

## API Contract

### Base URL
```
https://api.quickbite.local/v1
```

### Endpoints

#### 1. Get All Menu Items
```http
GET /menu-items?category={category}&page={page}&limit={limit}

Response 200:
{
  "data": [
    {
      "id": "uuid",
      "name": "Margherita Pizza",
      "description": "Fresh tomatoes, mozzarella, basil",
      "price": 12.99,
      "category": "mains",
      "ingredients": ["tomatoes", "mozzarella", "basil"],
      "dietaryTags": ["vegetarian"],
      "createdAt": "2025-09-20T10:00:00Z",
      "updatedAt": "2025-09-20T10:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

#### 2. Get Menu Item by ID
```http
GET /menu-items/{id}

Response 200:
{
  "data": {
    "id": "uuid",
    "name": "Margherita Pizza",
    "description": "Fresh tomatoes, mozzarella, basil",
    "price": 12.99,
    "category": "mains",
    "ingredients": ["tomatoes", "mozzarella", "basil"],
    "dietaryTags": ["vegetarian"],
    "createdAt": "2025-09-20T10:00:00Z",
    "updatedAt": "2025-09-20T10:00:00Z"
  }
}

Response 404:
{
  "error": {
    "code": "MENU_ITEM_NOT_FOUND",
    "message": "Menu item with ID {id} not found"
  }
}
```

#### 3. Create Menu Item
```http
POST /menu-items
Content-Type: application/json

Request Body:
{
  "name": "Margherita Pizza",
  "description": "Fresh tomatoes, mozzarella, basil",
  "price": 12.99,
  "category": "mains",
  "ingredients": ["tomatoes", "mozzarella", "basil"],
  "dietaryTags": ["vegetarian"]
}

Response 201:
{
  "data": {
    "id": "uuid",
    "name": "Margherita Pizza",
    "description": "Fresh tomatoes, mozzarella, basil",
    "price": 12.99,
    "category": "mains",
    "ingredients": ["tomatoes", "mozzarella", "basil"],
    "dietaryTags": ["vegetarian"],
    "createdAt": "2025-09-20T10:00:00Z",
    "updatedAt": "2025-09-20T10:00:00Z"
  }
}
```

#### 4. Update Menu Item
```http
PUT /menu-items/{id}
Content-Type: application/json

Request Body:
{
  "name": "Margherita Pizza Deluxe",
  "price": 14.99
}

Response 200:
{
  "data": {
    "id": "uuid",
    "name": "Margherita Pizza Deluxe",
    "description": "Fresh tomatoes, mozzarella, basil",
    "price": 14.99,
    "category": "mains",
    "ingredients": ["tomatoes", "mozzarella", "basil"],
    "dietaryTags": ["vegetarian"],
    "createdAt": "2025-09-20T10:00:00Z",
    "updatedAt": "2025-09-20T11:30:00Z"
  }
}
```

#### 5. Delete Menu Item
```http
DELETE /menu-items/{id}

Response 204: No Content
```

#### 6. AI Search Menu Items
```http
POST /menu-items/search
Content-Type: application/json

Request Body:
{
  "query": "spicy chicken dishes",
  "filters": {
    "dietaryTags": ["gluten-free"],
    "maxPrice": 20.00
  }
}

Response 200:
{
  "data": [
    {
      "id": "uuid",
      "name": "Spicy Buffalo Chicken",
      "description": "Grilled chicken with buffalo sauce",
      "price": 16.99,
      "category": "mains",
      "ingredients": ["chicken", "buffalo sauce", "celery"],
      "dietaryTags": ["gluten-free"],
      "relevanceScore": 0.95,
      "createdAt": "2025-09-20T10:00:00Z",
      "updatedAt": "2025-09-20T10:00:00Z"
    }
  ],
  "searchMeta": {
    "query": "spicy chicken dishes",
    "totalResults": 3,
    "searchTime": "45ms"
  }
}
```

## AI Integration

### AI Search Feature Overview
The AI search functionality uses semantic search to understand natural language queries and match them against menu items. The system analyzes menu item names, descriptions, and ingredients to provide relevant results.

### AI Behavior
- **Natural Language Processing**: Converts user queries like "healthy salad options" into searchable terms
- **Semantic Matching**: Understands synonyms (e.g., "spicy" matches "hot", "fiery")
- **Ingredient Intelligence**: Recognizes ingredient relationships (e.g., "cheese" includes "mozzarella", "cheddar")
- **Dietary Awareness**: Automatically filters based on dietary restrictions mentioned in queries
- **Fuzzy Matching**: Handles typos and variations in ingredient/dish names
- **Relevance Scoring**: Returns results ranked by relevance (0.0 to 1.0 scale)

### Example AI Queries
- `"vegetarian pasta with cheese"` → Returns vegetarian pasta dishes containing cheese
- `"spicy food under $15"` → Returns spicy items under $15
- `"gluten free desserts"` → Returns desserts tagged as gluten-free
- `"contains mushrooms"` → Returns items with mushrooms in ingredients list

## Tech Stack Suggestions

### Backend
- **.NET 7+**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Primary database
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation

### AI/Search
- **Azure Cognitive Search**: Semantic search capabilities
- **Alternative**: Elasticsearch with custom scoring
- **Fallback**: In-memory fuzzy search with FuzzySharp library

### Testing
- **xUnit**: Unit testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Local development orchestration
- **GitHub Actions**: CI/CD pipeline

## Database

### SQLite Data Storage
The application uses SQLite as the primary database for storing menu item data. SQLite provides a lightweight, serverless database solution that's perfect for development and training environments.

### Database Schema
- **MenuItems Table**: Stores all menu item information including ID, name, description, price, category, ingredients (JSON), dietary tags (JSON), and timestamps
- **Categories Table**: Reference table for valid menu categories (appetizers, mains, desserts, beverages)
- **Indexes**: Performance indexes on frequently queried fields (category, dietary tags, price range)

### Data Storage Benefits
- **Zero Configuration**: No separate database server setup required
- **Portable**: Database file can be easily copied and shared
- **ACID Compliance**: Ensures data integrity for concurrent operations
- **Full-Text Search**: Built-in FTS support for ingredient and description searches
- **JSON Support**: Modern SQLite versions support JSON data types for complex fields

### Connection Management
- **Entity Framework Core**: SQLite provider for ORM operations
- **Connection Pooling**: Automatic connection management
- **Migration Support**: Code-first database schema updates
- **Seeding**: Initial data population for development and testing

## Dev Workflow (TDD Approach)

### 1. Unit Tests to Write
- **MenuItem Model Tests**: Validation rules and business logic
- **Repository Tests**: CRUD operations with in-memory database
- **Service Layer Tests**: Business logic and AI search functionality
- **Controller Tests**: HTTP request/response handling
- **Validation Tests**: Input validation scenarios

### 2. Integration Tests to Write
- **API Endpoint Tests**: Full HTTP request/response cycles
- **Database Integration Tests**: Real database operations
- **AI Search Integration Tests**: End-to-end search functionality

### 3. TDD Workflow Steps
1. Write failing test for new feature
2. Write minimal code to make test pass
3. Refactor code while keeping tests green
4. Repeat for each feature increment

### 4. Test Categories
- **Fast Tests**: Unit tests (run on every build)
- **Medium Tests**: Integration tests (run on PR)
- **Slow Tests**: End-to-end tests (run on release)

## Deployment

### Docker Strategy
```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
```

### Container Requirements
- **Base Image**: mcr.microsoft.com/dotnet/aspnet:7.0
- **Environment Variables**: Database connection, AI service keys
- **Health Checks**: `/health` endpoint for container orchestration
- **Port Exposure**: 8080 (HTTP), 8081 (HTTPS)

### Docker Compose Services
- **API Service**: Main application container
- **Database Service**: SQL Server/PostgreSQL container
- **Cache Service**: Redis container (optional)

## Security & Risks

### Security Considerations
- **Input Validation**: Sanitize all user inputs to prevent injection attacks
- **Rate Limiting**: Implement API rate limiting to prevent abuse
- **CORS Configuration**: Configure appropriate CORS policies
- **Error Information**: Avoid exposing sensitive data in error messages
- **Logging Security**: Ensure no sensitive data in logs

### Technical Risks
- **AI Search Performance**: Large datasets may cause slow search responses
- **Database Scaling**: Single database may become bottleneck
- **Memory Usage**: In-memory search fallback may consume excessive RAM
- **Third-party Dependencies**: AI service outages may break search functionality

### Mitigation Strategies
- Implement caching for AI search results
- Add database connection pooling and read replicas
- Set memory limits and implement result pagination
- Provide graceful degradation when AI services unavailable
- Add comprehensive monitoring and alerting

---

**Document Version**: 1.0  
**Last Updated**: September 20, 2025  
**Next Review**: October 20, 2025