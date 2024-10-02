The "S3 Bucket Operations" module (Special Execution Task/SET) for Tosca facilitates interactions with Amazon S3 buckets to perform common operations on S3 buckets directly within their automated test scripts. This module is implemented as a Special Execution Task (SET) and supports various operations such as uploading files, downloading files, listing files, deleting files, and checking the existence of files in an S3 bucket. It provides a flexible and robust way to automate S3 operations in test cases.

**Key Functionalities:**

**Upload File:** Allows users to upload a local file to a specified S3 bucket. The file is stored with a designated key, combining the S3 file path and file name.

**Download File:** Facilitates downloading a file from a specified S3 bucket to a local directory. The module ensures the file is retrieved using the correct S3 path and file name.

**List Files:** Lists all files present in a specified S3 bucket. This operation is useful for validating the contents of the bucket or ensuring that a file has been successfully uploaded.

**Delete File:** Deletes a specified file from the S3 bucket. This operation helps in maintaining the bucket by removing unnecessary or old files.

**Check File Exists:** Checks whether a specific file exists in the S3 bucket. This operation is vital for verifying that a file is available before attempting to download or manipulate it.

**Technical Details:**
The code in this repo / SET dll, is created on
  - .Net SDK Version 8.0
  - .Net Framework 4.8
  - Visual Studio 22
  - Windows

**How to use this SET/Module ?**
  1. Download the attached zip file, Unzip   (**Download link:**  _https://github.com/prakashnarkhede/TOSCA-SpecialExecutionTask--S3-Bucket-Operations/blob/master/S3%20Bucket%20Operations%20(TOSCA-%20Special%20Execution%20Task).zip_)
  2. copy all the dll and xml files to TBOX folder (ex - C:\Program Files (x86)\TRICENTIS\Tosca Testsuite\TBox )
  3. Restart TOSCA Commander
  4. Import the module from .tsu file
  5. Enter necessary information in the test steps
     
      ** a. ** Set the S3Bucket_Name, S3Bucket_Region, S3Bucket_AccessKey, and S3Bucket_SecretAccess parameters with appropriate values.

      ** b. ** Specify the Operation parameter as required. Supported operations with this SET are - Upload File, Download File, List Files, Delete File and check if file exists.

     **  c. ** module attribute output to be used to buffer file lists in case of List Files Operation, and Check if file exists, it return true or false.

**Module (xModule/Special Execution Task):**
![image](https://github.com/user-attachments/assets/403281ab-f6a9-4627-8f1c-08bc0c1c0b90)

**Test Case Example:**
![image](https://github.com/user-attachments/assets/31579303-907c-4f81-99b3-f9faba0479a3)

Refer to below youtube videos to understand the detailed implementation of the above SET

Video#1. Automating AWS S3 Bucket Operations in Tricentis Tosca: Overview, Use Cases, and Demo
https://www.youtube.com/watch?v=_ZL8KnHRKdI

Video#2. AWS S3 Bucket Operations with  Tosca: Part1 (Setup - Visual Studio & Module Structure Guide)
https://www.youtube.com/watch?v=-jFRMeIAK34

Video#3. AWS S3 Bucket Operations with Tosca: Part2 (Detailed Implementation & Integration) 
https://youtu.be/vfui-o2T904


