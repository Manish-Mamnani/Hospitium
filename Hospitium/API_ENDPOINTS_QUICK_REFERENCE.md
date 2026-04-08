# Complete Hospitium API Endpoints - All Services

**Total Microservices:** 6  
**Total API Endpoints:** 21  
**Framework:** .NET 8  
**Gateway:** Ocelot

---

## 📋 Quick Reference Table

| # | Service | Method | Endpoint | Auth | Description |
|---|---------|--------|----------|------|-------------|
| 1 | Auth | POST | `/api/auth/register` | ❌ | Register new user |
| 2 | Auth | POST | `/api/auth/login` | ❌ | Login & get JWT |
| 3 | Users | GET | `/api/users/managers` | ⚠️ | Get all managers |
| 4 | Hotel | POST | `/api/hotels` | ✅ HM | Create hotel |
| 5 | Hotel | POST | `/api/hotels/rooms` | ✅ HM | Add room |
| 6 | Hotel | GET | `/api/hotels` | ❌ | Search/filter hotels |
| 7 | Hotel | GET | `/api/hotels/{id}` | ❌ | Get hotel details |
| 8 | Hotel | GET | `/api/hotels/{id}/rooms` | ❌ | Get rooms |
| 9 | Hotel | GET | `/api/hotels/rooms/{roomId}` | ❌ | Check availability |
| 10 | Hotel | PUT | `/api/hotels/rooms/{roomId}` | ✅ HM | Update room |
| 11 | Hotel | PUT | `/api/hotels/{id}/approve` | ✅ A | Approve hotel |
| 12 | Hotel | PUT | `/api/hotels/{id}/reject` | ✅ A | Reject hotel |
| 13 | Hotel | GET | `/api/hotels/my` | ✅ HM | Manager's hotels |
| 14 | Booking | POST | `/api/bookings` | ✅ U | Create booking |
| 15 | Booking | GET | `/api/bookings/my` | ✅ U | User's bookings |
| 16 | Booking | GET | `/api/bookings` | ✅ A | All bookings |
| 17 | Booking | GET | `/api/bookings/{id}` | ✅ U | Booking details |
| 18 | Booking | PUT | `/api/bookings/{id}/cancel` | ✅ U | Cancel booking |
| 19 | Review | POST | `/api/reviews` | ✅ U | Add review |
| 20 | Review | POST | `/api/reviews/rate` | ✅ U | Quick rate |
| 21 | Review | GET | `/api/reviews/hotel/{id}` | ❌ | Get reviews |

**Legend:**  
❌ = Public | ⚠️ = Optional | ✅ U = User | ✅ A = Admin | ✅ HM = HotelManager

---

## 🔗 Service Base URLs

```
Gateway:        http://localhost:5000
AuthService:    http://localhost:5001
HotelService:   http://localhost:5002
BookingService: http://localhost:5003
ReviewService:  http://localhost:5005
```

---

## 📊 Services Breakdown

### 1. Authentication Service (Port 5001)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login & get JWT token

### 2. Users Service (Port 5001)
- `GET /api/users/managers` - Get all hotel managers

### 3. Hotel Service (Port 5002)
- `POST /api/hotels` - Create hotel (HotelManager)
- `POST /api/hotels/rooms` - Add room (HotelManager)
- `GET /api/hotels` - Search/filter/sort/paginate hotels
- `GET /api/hotels/{id}` - Get hotel details
- `GET /api/hotels/{id}/rooms` - Get all rooms in hotel
- `GET /api/hotels/rooms/{roomId}` - Check room availability
- `PUT /api/hotels/rooms/{roomId}` - Update room (HotelManager)
- `PUT /api/hotels/{id}/approve` - Approve hotel (Admin)
- `PUT /api/hotels/{id}/reject` - Reject hotel (Admin)
- `GET /api/hotels/my` - Get manager's hotels (HotelManager)

### 4. Booking Service (Port 5003)
- `POST /api/bookings` - Create booking
- `GET /api/bookings/my` - Get user's bookings
- `GET /api/bookings` - Get all bookings (Admin)
- `GET /api/bookings/{id}` - Get booking details
- `PUT /api/bookings/{id}/cancel` - Cancel booking

### 5. Review Service (Port 5004)
- `POST /api/reviews` - Add full review with comment
- `POST /api/reviews/rate` - Quick rating only
- `GET /api/reviews/hotel/{hotelId}` - Get hotel reviews

---

## 🔐 Role-Based Access

| Role | Services | Endpoints |
|------|----------|-----------|
| **User** | Auth, Booking, Review | Register, Login, Create Booking, View Own Bookings, Cancel Booking, Add Review |
| **Admin** | Hotel, Booking | Approve/Reject Hotels, View All Bookings |
| **HotelManager** | Auth, Hotel | Create Hotel, Create/Update Rooms, View Own Hotels |

---

## 📱 Sample cURL Commands

### Register User
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### Login
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

### Search Hotels
```bash
curl -X GET "http://localhost:5002/api/hotels?search=Grand&city=NewYork&page=1&pageSize=10" \
  -H "Accept: application/json"
```

### Create Booking
```bash
curl -X POST http://localhost:5003/api/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "roomId": 1,
    "fromDate": "2024-02-15T00:00:00Z",
    "toDate": "2024-02-20T00:00:00Z"
  }'
```

### Add Review
```bash
curl -X POST http://localhost:5004/api/reviews \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "hotelId": 1,
    "rating": 5,
    "comment": "Excellent hotel!"
  }'
```

### Get Hotel Reviews
```bash
curl -X GET http://localhost:5004/api/reviews/hotel/1 \
  -H "Accept: application/json"
```

---

## 🔄 Event Flow

```
1. User Registration
   AuthService → publishes UserRegisteredEvent
   NotificationService → sends welcome email

2. Hotel Booking
   BookingService → publishes BookingCreatedEvent
   HotelService → decrements room availability
   NotificationService → sends confirmation email

3. Review Added
   ReviewService → publishes ReviewAddedEvent
   HotelService → updates hotel average rating

4. Booking Cancelled
   BookingService → publishes BookingCancelledEvent
   HotelService → increments room availability
   NotificationService → sends cancellation email
```

---

## 📝 Common Error Responses

| Code | Meaning | Common Causes |
|------|---------|---------------|
| 200 | OK | Request successful |
| 400 | Bad Request | Validation failure, invalid input |
| 401 | Unauthorized | Missing/invalid token, wrong role |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Email exists, date conflict |
| 500 | Internal Server Error | Server error |

---

## 📚 Documentation Files

- **API_ENDPOINTS.md** - Complete detailed API reference with request/response examples
- **REVIEW_SERVICE_SUMMARY.md** - Focused review service documentation
- **CODEBASE_DOCUMENTATION.md** - Complete codebase architecture and structure

---

**Last Updated:** 2024  
**Version:** 3.0 (Added ReviewService)
