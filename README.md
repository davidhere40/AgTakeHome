# AgTakeHome

#Brief Description
This is a simple file storage API service using .Net Core 6, SQL Lite, and Entity Framework. It allows binary files to be saved with a name, and keeps track of previous versions. The solution also includes a unit test project.

Swagger Documentation can be found at http://localhost:5000/swagger/index.html when the application is running.

#Instructions
To test the project, open the solution file in visual studio and run the unit tests from the project FileStorageUnitTests.
To run the web app locally, you can run the project from visual studio. It's not currently designed to be deployed for production.

## Get list of Files

### Request

	`GET /Files/`
	
	No parameters
	
	Gets the list of files without the binary data
	
### Response

    A list of files with the following data types: 
		string Name, 
		int Version, 
		bool IsLatest, 
		DateTime CreateDate
	
## Create a File

### Request

	`Post /Files/Create?name={name}`
	
	Query String Parameters:
		Name: an alpha-numeric file name.
	Body Parameters:
		Data: The binary data of a file
		
	Creates a new File. The resulting file has IsLatest set to true and a create date of the date created on the server.
	
### Response

    Success or Failure
	
	Example:
		HTTP/1.1 200 OK
		Date: 
		Status: 200 OK
		Connection: close
		Content-Type: application/json
		Content-Length: 2

		[]
		
## Update a File

### Request

	`PUT /Files/Update?name={name}`
	
	Query String Parameters:
		Name: an alpha-numeric file name.
	Body Parameters:
		Data: The binary data of a file
		
	When a file is updated, the version number is incremented and a new file record is created. The old one is not deleted. The latest file's IsLatest column is set to true and the old file's IsLatest column is set to false. The create date of the new file is the create date of the updated file. The old file version keeps it's original create date.
	
### Response

    Success or Failure
	
	Example:
		HTTP/1.1 200 OK
		Date: 
		Status: 200 OK
		Connection: close
		Content-Type: application/json
		Content-Length: 2

		[]

## Delete a File

### Request

	`DELETE /Files/Delete?name={name}`
	
	Query String Parameters:
		Name: an alpha-numeric file name.
	
### Response

    Success or Failure
	
	Example:
		HTTP/1.1 200 OK
		Date: 
		Status: 200 OK
		Connection: close
		Content-Type: application/json
		Content-Length: 2

		[]

## Get a File by Name

### Request

	`GET /Files/GetByName?name={name}`
	
	Query String Parameters:
		Name: an alpha-numeric file name.
		
	Gets the file details and it's binary data.
	
### Response

    The file data with the following data types: 
		int Id, 
		string Name, 
		byte[] Data, 
		int Version, 
		bool IsLatest, 
		DateTime CreateDate



