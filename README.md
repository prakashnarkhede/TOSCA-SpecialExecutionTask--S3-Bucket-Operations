The "S3 Bucket Operations" module (Special Execution Task/SET) for Tosca is to facilitate interactions with Amazon S3 bucket to perform common operations on S3 buckets directly within their automated test scripts. This module is implemented as a Special Execution Task (SET) and supports various operations such as uploading files, downloading files, listing files, deleting files, and checking the existence of files in an S3 bucket. It provides a flexible and robust way to automate S3 operations in test cases.

**Key Functionalities:**

**Upload File:** Allows users to upload a local file to a specified S3 bucket. The file is stored with a designated key, combining the S3 file path and file name.

**Download File:** Facilitates downloading a file from a specified S3 bucket to a local directory. The module ensures the file is retrieved using the correct S3 path and file name.

**List Files:** Lists all files present in a specified S3 bucket. This operation is useful for validating the contents of the bucket or ensuring that a file has been successfully uploaded.

**Delete File:** Deletes a specified file from the S3 bucket. This operation helps in maintaining the bucket by removing unnecessary or old files.

**Check File Exists:** Checks whether a specific file exists in the S3 bucket. This operation is vital for verifying that a file is available before attempting to download or manipulate it.


**How to use this SET/Module ?**
  1. Download the attached zip file, Unzip  (Download link: )
  2. copy all the dll and xml files to TBOX folder (ex - C:\Program Files (x86)\TRICENTIS\Tosca Testsuite\TBox )
  3. Import the module from .tsu file
  4. Enter necessary information in the test steps
       a. Set the S3Bucket_Name, S3Bucket_Region, S3Bucket_AccessKey, and S3Bucket_SecretAccess parameters with appropriate values.
       b. Specify the Operation parameter as required. Supported operations with this SET are - Upload File, Download File, List Files, Delete File and check if file exists.
       c. module attribute output to be used to buffer file lists in case of List Files Operation, and Check if file exists, it return true or false.

**Module (xModule/Special Execution Task):**

![image](https://github.com/user-attachments/assets/c339f345-3416-463c-acf0-23651f763e4c)

**Test Case Example:**

![image](https://github.com/user-attachments/assets/521d5b19-aa65-4f23-abbf-65c2e2875bcc)

