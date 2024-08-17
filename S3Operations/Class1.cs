using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Tricentis.Automation.Engines.SpecialExecutionTasks.Attributes;
using Tricentis.Automation.Engines.SpecialExecutionTasks;
using Tricentis.Automation.Creation;
using Tricentis.Automation.Engines;
using Tricentis.Automation.AutomationInstructions.TestActions;
using Tricentis.Automation.AutomationInstructions.Dynamic.Values;

namespace S3Operations
{
    [SpecialExecutionTaskName("S3_Bucket_Operations")]
    class S3_Bucket_Operations_Engine : SpecialExecutionTask
    {
        private readonly List<string> _logs = new List<string>(); // List to store logs

        public S3_Bucket_Operations_Engine(Validator validator) : base(validator)
        {
        }
        public override ActionResult Execute(ISpecialExecutionTaskTestAction testAction)
        {
            try
            {
                Log("Starting S3 Bucket Operations Execution...");
                IParameter configParameters = testAction.GetParameter("S3Bucket_Configurations", true);
                if (configParameters == null)
                {
                    Log("Configuration parameters are missing.");
                    return new UnknownFailedActionResult("Configuration parameters are missing.");
                }
                Log("Fetching S3 bucket configuration parameters...");
                IInputValue IbucketName = configParameters.GetChildParameter("S3Bucket_Name", true, new[] { ActionMode.Input }).GetAsInputValue();
                IInputValue Iregion = configParameters.GetChildParameter("S3Bucket_Region", true, new[] { ActionMode.Input }).GetAsInputValue();
                IInputValue IaccessKey = configParameters.GetChildParameter("S3Bucket_AccessKey", true, new[] { ActionMode.Input }).GetAsInputValue();
                IInputValue IsecretKey = configParameters.GetChildParameter("S3Bucket_SecretAccess", true, new[] { ActionMode.Input }).GetAsInputValue();

                if (IbucketName == null) Log("S3 Bucket Name parameter is missing.");
                if (Iregion == null) Log("S3 Bucket Region parameter is missing.");
                if (IaccessKey == null) Log("S3 Access Key parameter is missing.");
                if (IsecretKey == null) Log("S3 Secret Key parameter is missing.");


                if (IbucketName == null || Iregion == null || IaccessKey == null || IsecretKey == null)
                    return new UnknownFailedActionResult("One or more required S3 bucket configuration parameters are missing.");

                Log("Fetching operation parameters...");

                IInputValue Ioperation = testAction.GetParameterAsInputValue("Operation", false);
                IInputValue IlocalFilePath = testAction.GetParameterAsInputValue("LocalFilePath", true);
                IInputValue IlocalFileName = testAction.GetParameterAsInputValue("LocalFileName", true);
                IInputValue Is3_FilePath = testAction.GetParameterAsInputValue("S3_FilePath", true);
                IInputValue Is3_FileName = testAction.GetParameterAsInputValue("S3_FileName", true);
                IInputValue Ioutput = testAction.GetParameterAsInputValue("Output", true);

                if (Ioperation == null) Log("Operation parameter is missing.");
                if (IlocalFilePath == null) Log("Local File Path parameter is missing.");
                if (IlocalFileName == null) Log("Local File Name parameter is missing.");
                if (Is3_FilePath == null) Log("S3 File Path parameter is missing.");
                if (Is3_FileName == null) Log("S3 File Name parameter is missing.");

                if (Ioperation == null || IlocalFilePath == null || IlocalFileName == null || Is3_FilePath == null || Is3_FileName == null)
                    return new UnknownFailedActionResult("One or more required operation parameters are missing.");


                string bucketName = IbucketName.Value;
                string region = Iregion.Value;
                string accessKey = IaccessKey.Value;
                string secretKey = IsecretKey.Value;
                string operation = Ioperation.Value;
                string LocalFilePath = IlocalFilePath.Value;
                string LocalFileName = IlocalFileName.Value;
                string S3_FilePath = Is3_FilePath.Value;
                string S3_FileName = Is3_FileName.Value;
                string Output = String.Empty; //output variable is for returning the output data, to be utilized for buffering/verifying
                IAmazonS3 s3Client;

                // Initialize the Amazon S3 client with the provided access key, secret key, and region
                Log("Initializing Amazon S3 client...");
                RegionEndpoint bucketRegion = RegionEndpoint.GetBySystemName(region);
                s3Client = new AmazonS3Client(accessKey, secretKey, bucketRegion);
                Log("Amazon S3 client initialized.");

                string localFilePath = Path.Combine(LocalFilePath, LocalFileName);
                string s3Key = $"{S3_FilePath}/{S3_FileName}";

                string key = LocalFileName;
                Log($"Operation: {operation}");
                Log($"Local file full path: {localFilePath}");
                Log($"S3 bucket name: {bucketName}");
                Log($"S3 file key: {s3Key}");

                switch (operation)
                {
                    case "Upload File":
                        Task<ActionResult> task = UploadFileAsync(s3Client, bucketName, localFilePath, s3Key);
                        return task.Result;
                    case "Download File":
                        Task<ActionResult> task1 = DownloadFileAsync(s3Client, bucketName, s3Key, LocalFilePath);
                        return task1.Result;
                    case "List Files":
                        ListFilesAsync(s3Client, bucketName).Wait();
                        return new PassedActionResult("Files listed successfully.\n" + GetLogs());
                    case "Delete File":
                        DeleteFileAsync(s3Client, bucketName, key).Wait();
                        return new PassedActionResult("File deleted successfully.\n" + GetLogs());
                    case "Check File Exists":
                        FileExistsAsync(s3Client, bucketName, key).Wait();
                        return new PassedActionResult("File existence check completed.\n" + GetLogs());
                    case "Generate Pre-Signed URL":
                        GeneratePreSignedURL(s3Client, bucketName, key).Wait();
                        return new PassedActionResult("Pre-signed URL generated successfully.\n" + GetLogs());
                    default:
                        Log("Invalid operation specified.");
                        return new UnknownFailedActionResult("File Read Completed. All Respective actions completed..");
                        ;
                }
            }
            catch (Exception ex)
            {
                Log($"An error occurred during execution: {ex.Message}");
                return new UnknownFailedActionResult(GetLogs());
            }
        }
        private async Task<ActionResult> UploadFileAsync(IAmazonS3 s3Client, string bucketName, string filePath, string key)
        {
            try
            {
                Log($"Starting upload of file: {filePath} to S3 bucket: {bucketName} with key: {key}");
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(filePath, bucketName, key);
                Log("Upload completed successfully.");
                return new PassedActionResult($"File uploaded to S3 successfully. S3 Path: {key}\n" + GetLogs());
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during upload: {e.Message}");
                return new UnknownFailedActionResult($"Error during upload. S3 Path: {key}. Error: {e.Message}\n" + GetLogs());
            }
            catch (Exception e)
            {
                Log($"General error during upload: {e.Message}");
                return new UnknownFailedActionResult($"General error during upload. S3 Path: {key}. Error: {e.Message}\n" + GetLogs());
            }
        }


        private async Task<ActionResult> DownloadFileAsync(IAmazonS3 s3Client, string bucketName, string key, string destinationPath)
        {
            try
            {
                Log($"Starting download of file from S3 bucket: {bucketName} with key: {key} to local path: {destinationPath}");
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.DownloadAsync(destinationPath, bucketName, key);
                Log("Download completed successfully.");
                return new PassedActionResult($"File downloaded successfully. Local Path: {destinationPath}\n" + GetLogs());
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during download: {e.Message}");
                return new UnknownFailedActionResult($"Error during download. S3 Path: {key}. Error: {e.Message}\n" + GetLogs());
            }
            catch (Exception e)
            {
                Log($"General error during download: {e.Message}");
                return new UnknownFailedActionResult($"General error during download. S3 Path: {key}. Error: {e.Message}\n" + GetLogs());
            }
        }


        private async Task ListFilesAsync(IAmazonS3 s3Client, string bucketName)
        {
            try
            {
                Log($"Starting file listing in S3 bucket: {bucketName}");
                var request = new ListObjectsRequest { BucketName = bucketName };
                var response = await s3Client.ListObjectsAsync(request);

                if (response.S3Objects.Count > 0)
                {
                    Log("Files found in the bucket:");
                    foreach (S3Object entry in response.S3Objects)
                    {
                        Log($" - {entry.Key} (size: {entry.Size} bytes)");
                    }
                }
                else
                {
                    Log("No files found in the bucket.");
                }

                Log("File listing completed successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during file listing: {e.Message}");
            }
            catch (Exception e)
            {
                Log($"General error during file listing: {e.Message}");
            }
        }

        private async Task DeleteFileAsync(IAmazonS3 s3Client, string bucketName, string key)
        {
            try
            {
                Log($"Starting deletion of file with key: {key} from S3 bucket: {bucketName}");
                var deleteObjectRequest = new DeleteObjectRequest { BucketName = bucketName, Key = key };
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
                Log("File deletion completed successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during file deletion: {e.Message}");
            }
            catch (Exception e)
            {
                Log($"General error during file deletion: {e.Message}");
            }
        }

        private async Task FileExistsAsync(IAmazonS3 s3Client, string bucketName, string key)
        {
            try
            {
                Log($"Checking existence of file with key: {key} in S3 bucket: {bucketName}");
                var request = new GetObjectMetadataRequest { BucketName = bucketName, Key = key };
                var response = await s3Client.GetObjectMetadataAsync(request);
                Log($"File exists. Last modified: {response.LastModified}");
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during file existence check: {e.Message}");
            }
            catch (Exception e)
            {
                Log($"General error during file existence check: {e.Message}");
            }
        }

        private async Task GeneratePreSignedURL(IAmazonS3 s3Client, string bucketName, string key)
        {
            try
            {
                Log($"Generating pre-signed URL for file with key: {key} in S3 bucket: {bucketName}");
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                };
                string url = s3Client.GetPreSignedURL(request);
                Log($"Pre-Signed URL: {url}");
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during pre-signed URL generation: {e.Message}");
            }
            catch (Exception e)
            {
                Log($"General error during pre-signed URL generation: {e.Message}");
            }
        }

        private void Log(string message)
        {
            _logs.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        private string GetLogs()
        {
            return string.Join(Environment.NewLine, _logs);
        }
    }
}
