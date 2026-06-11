# ImageDownloader.API

## Overview

ImageDownloader.API is a .NET 8 Web API that downloads multiple images asynchronously from a list of URLs and stores them locally on the server.
The API supports configurable concurrent downloads using `SemaphoreSlim`, ensuring that only a specified number of images are downloaded at the same time.
The project also provides an endpoint to retrieve a previously downloaded image as a Base64 string.

---

## Features

* Download multiple images asynchronously
* Configurable maximum concurrent downloads
* Local image storage
* Unique file name generation using GUIDs
* Retrieve stored images as Base64 strings
* CancellationToken support
* Structured logging
* Memory-efficient file streaming

---

## Project Structure

```text
ImageDownloader.API
│
├── Controllers
│   └── ImageController.cs
│
├── Interfaces
│   └── IImageService.cs
│
├── Models
│   ├── RequestedDownload.cs
│   ├── ResponseDownload.cs
│   
│
├── Services
│   └── ImageService.cs
│
├── DownloadedImages
│
└── Program.cs
```

---

## API Endpoints

### Download Images

**POST** `/api/image/download`

#### Request

```json
{
  "imageUrls": [
    "https://picsum.photos/id/1/400/300",
    "https://picsum.photos/id/2/400/300",
    "https://picsum.photos/id/3/400/300"
  ],
  "maxDownloadAtOnce": 2
}
```

#### Response

```json
{
  "success": true,
  "message": "Images processed successfully.",
  "urlAndNames": {
    "https://picsum.photos/id/1/400/300": "9b1252fa-2a4f-4c09-a2d0-3ce43e0408e6.jpg",
    "https://picsum.photos/id/2/400/300": "411b647c-0a77-4dc5-a5fc-6abd0d04a608.jpg",
    "https://picsum.photos/id/3/400/300": "f6b64665-c4cf-45cd-bed6-14e76831aecf.jpg"
  }
}
```

---

### Get Image By Name

**GET** `/api/image/get-image-by-name/{imageName}`

#### Response

```json
{
  "fileName": "9b1252fa-2a4f-4c09-a2d0-3ce43e0408e6.jpg",
  "base64String": "..."
}
```

---

## Running the Project

### Prerequisites

* .NET 8 SDK

### Run

```bash
git clone <repository-url>
cd ImageDownloader.API
dotnet restore
dotnet run
```

Swagger will be available at:

```text
https://localhost:{port}/swagger
```

---

## Design Decisions

### Why SemaphoreSlim?

The assignment requires limiting the number of simultaneous downloads.

`SemaphoreSlim` is used to ensure that only `MaxDownloadAtOnce` download operations can run concurrently.

Example:

```text
Total Images: 10
MaxDownloadAtOnce: 3

Running:
1, 2, 3

When one finishes:
4 starts

When another finishes:
5 starts
```

This provides controlled parallelism while avoiding excessive resource consumption.

---

### Memory Management

To minimize memory usage:

* Images are streamed directly to disk.
* Files are not loaded entirely into memory during download.
* HttpClientFactory is used to manage HttpClient instances efficiently.

---

## Technologies Used

* .NET 8 Web API
* C#
* HttpClientFactory
* SemaphoreSlim
* Swagger

---

## Notes

* Downloaded files are stored under the `DownloadedImages` folder.
* File names are generated using GUIDs to avoid collisions.
* Invalid or inaccessible URLs are handled gracefully and reported in the response.
