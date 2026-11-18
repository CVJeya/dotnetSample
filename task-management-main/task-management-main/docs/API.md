# Task Management API — Quick Reference

Base URL (local): `http://localhost:5000/api`

## JSON naming

Responses use **camelCase** property names (`taskTitle`, `taskDueDate`, …) via ASP.NET Core’s default JSON serializer.

## `GET /tasks`

Returns `200 OK` with an array of tasks.

## `GET /tasks/{id}`

Returns `200 OK` or `404 Not Found`.

## `GET /tasks/search?keyword=`

`keyword` may be empty — behavior matches list (all tasks).

## `POST /tasks`

**Request (`CreateTaskDto`):**

```json
{
  "taskTitle": "Implement reporting module",
  "taskDescription": "Add filters and CSV export.",
  "taskDueDate": "2026-06-01T00:00:00.000Z",
  "taskStatus": "Pending",
  "taskRemarks": "Stakeholder review pending.",
  "createdBy": "jdoe"
}
```

**Responses:** `201 Created` with body + `Location` header, or `400 Bad Request` with validation errors.

## `PUT /tasks/{id}`

**Request (`UpdateTaskDto`):**

```json
{
  "taskTitle": "Implement reporting module",
  "taskDescription": "Add filters and CSV export.",
  "taskDueDate": "2026-06-15T00:00:00.000Z",
  "taskStatus": "InProgress",
  "taskRemarks": "Kick-off scheduled.",
  "lastUpdatedBy": "jdoe"
}
```

**Responses:** `200 OK`, `400 Bad Request`, or `404 Not Found`.

## `DELETE /tasks/{id}`

**Responses:** `204 No Content` or `404 Not Found`.

## Errors

Unhandled exceptions are normalized by `GlobalExceptionMiddleware` to JSON:

```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred.",
  "timestamp": "2026-05-07T18:00:00.000Z",
  "detail": null
}
```

In Development, `detail` may contain exception diagnostics.
