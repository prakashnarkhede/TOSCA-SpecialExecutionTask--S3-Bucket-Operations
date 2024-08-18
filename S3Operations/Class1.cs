using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Tricentis.Automation.Engines.SpecialExecutionTasks.Attributes;
using Tricentis.Automation.Engines.SpecialExecutionTasks;
using Tricentis.Automation.Creation;
using Tricentis.Automation.Engines;
using Tricentis.Automation.AutomationInstructions.TestActions;
using Tricentis.Automation.AutomationInstructions.Dynamic.Values;
using Tricentis.Automation.AutomationInstructions.Configuration;
using Tricentis.Automation.Execution.Results;

namespace S3Operations
{
    [SpecialExecutionTaskName("S3_Bucket_Operations1")]
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
                    Log("S3 Configuration parameters are missing.");
                    return new UnknownFailedActionResult("S3 Configuration parameters are missing.");
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

                if (Ioperation == null) Log("Operation parameter is missing.");

                if (Ioperation == null)
                    return new UnknownFailedActionResult("operation parameters is missing.");

                string bucketName = IbucketName.Value;
                string region = Iregion.Value;
                string accessKey = IaccessKey.Value;
                string secretKey = IsecretKey.Value;
                string operation = Ioperation.Value;

                IAmazonS3 s3Client;

                // Initialize the Amazon S3 client with the provided access key, secret key, and region
                Log("Initializing Amazon S3 client...");
                RegionEndpoint bucketRegion = RegionEndpoint.GetBySystemName(region);
                s3Client = new AmazonS3Client(accessKey, secretKey, bucketRegion);
                Log("Amazon S3 client initialized.");

                Log($"Operation: {operation}");
                Log($"S3 bucket name: {bucketName}");

                switch (operation)
                {
                    case "Upload File":
                        Task<ActionResult> task = UploadFileAsync(s3Client, bucketName, testAction);
                        return task.Result;
                    case "Download File":
                      //  Task<ActionResult> task1 = DownloadFileAsync(s3Client, bucketName, testAction);
                       // return task1.Result;
                    case "List Files":
                        Task<ActionResult> task2 = ListFilesAsync(s3Client, bucketName, testAction);
                        return task2.Result;
                    case "Delete File":
                      //  DeleteFileAsync(s3Client, bucketName, testAction).Wait();
                       // return new PassedActionResult("File deleted successfully.\n" + GetLogs());
                    case "Check File Exists":
                        Task<ActionResult> taskCheckIfExist = FileExistsAsync(s3Client, bucketName, testAction);
                        return taskCheckIfExist.Result;
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
        private async Task<ActionResult> UploadFileAsync(IAmazonS3 s3Client, string bucketName, ISpecialExecutionTaskTestAction testAction)
        {
            string localFilePath;
            string s3Key = String.Empty;
            try
            {
                IInputValue IlocalFilePath = testAction.GetParameterAsInputValue("LocalFilePath", true);
                IInputValue IlocalFileName = testAction.GetParameterAsInputValue("LocalFileName", true);
                IInputValue Is3_FilePath = testAction.GetParameterAsInputValue("S3_FilePath", true);
                IInputValue Is3_FileName = testAction.GetParameterAsInputValue("S3_FileName", true);

                if (IlocalFilePath == null) Log("Local File Path parameter is missing.");
                if (IlocalFileName == null) Log("Local File Name parameter is missing.");
                if (Is3_FilePath == null) Log("S3 File Path parameter is missing.");
                if (Is3_FileName == null) Log("S3 File Name parameter is missing.");

                if ( IlocalFilePath == null || IlocalFileName == null || Is3_FilePath == null || Is3_FileName == null)
                    return new UnknownFailedActionResult("One or more required operation parameters are missing for upload file action, Local File Path, Local File Name, S3 file path and file name are mandatory for this action.");

                string LocalFilePath = IlocalFilePath.Value;
                string LocalFileName = IlocalFileName.Value;
                string S3_FilePath = Is3_FilePath.Value;
                string S3_FileName = Is3_FileName.Value;
                string Output = String.Empty; //output variable is for returning the output data, to be utilized for buffering/verifying

                localFilePath = Path.Combine(LocalFilePath, LocalFileName);
                s3Key = $"{S3_FilePath}/{S3_FileName}";

                Log($"Starting upload of file: {localFilePath} to S3 bucket: {bucketName} with key: {s3Key}");
                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(localFilePath, bucketName, s3Key);
                Log("Upload completed successfully.");
                return new PassedActionResult($"File uploaded to S3 successfully. S3 Path: {s3Key}\n" + GetLogs());
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during upload: {e.Message}");
                return new UnknownFailedActionResult($"Error during upload. S3 Path: {s3Key}. Error: {e.Message}\n" + GetLogs());
            }
            catch (Exception e)
            {
                Log($"General error during upload: {e.Message}");
                return new UnknownFailedActionResult($"General error during upload. S3 Path: {s3Key}. Error: {e.Message}\n" + GetLogs());
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


        private async Task<ActionResult> ListFilesAsync(IAmazonS3 s3Client, string bucketName, ISpecialExecutionTaskTestAction testAction)
        {
            try
            {
                IInputValue Is3_FilePath = testAction.GetParameterAsInputValue("S3_FilePath", true);
                string s3FilePath = Is3_FilePath?.Value?.Trim();

                if (Is3_FilePath == null)
                    return new UnknownFailedActionResult("S3_FilePath parameter is mandatory for ListFiles action.");

                Log($"Starting file listing in S3 bucket: {bucketName} with path: {s3FilePath ?? "root"}");

                // Correctly set the BucketName and Prefix properties
                var request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = string.IsNullOrEmpty(s3FilePath) ? null : s3FilePath // Use the prefix to filter by path
                }; var response = await s3Client.ListObjectsAsync(request);
                string filesList;
                if (response.S3Objects.Count > 0)
                {
                    Log("Files found in the bucket:");
                    List<string> files = new List<string>();
                    foreach (S3Object entry in response.S3Objects)
                    {
                        string fileInfo = $"{entry.Key} (size: {entry.Size} bytes)";
                        files.Add(fileInfo);
                        Log($" - {fileInfo}");
                    }
                    filesList = string.Join("; ", files);
                }
                else
                {
                    Log("No files found in the bucket.");
                    filesList = $"No Files Available at given location - s3://{bucketName}/{s3FilePath ?? ""}";
                }

                Log("File listing completed successfully.");

                // Store the result in the logs or return it as part of the ActionResult if needed
                Log($"Files List: {filesList}");
                IParameter outputParm = testAction.GetParameter("Output", true);
                if (outputParm.ActionMode == ActionMode.Buffer)
                {
                    IInputValue inputValue = outputParm.GetAsInputValue();
                    Buffers.Instance.SetBuffer(inputValue.Value, filesList, false);
                    testAction.SetResultForParameter(outputParm, SpecialExecutionTaskResultState.Ok, string.Format("Buffer {0} set to value {1}.", inputValue.Value, filesList));
                }
                else
                {
                    HandleActualValue(testAction, outputParm, Buffers.Instance.GetBuffer(outputParm.Name));
                }

                return new PassedActionResult($"File listing completed successfully.\nFiles List: {filesList}\n" + GetLogs());
            }
            catch (AmazonS3Exception e)
            {
                Log($"AWS S3 error during file listing: {e.Message}");
                return new UnknownFailedActionResult($"AWS S3 error during file listing: {e.Message}\n" + GetLogs());
            }
            catch (Exception e)
            {
                Log($"General error during file listing: {e.Message}");
                return new UnknownFailedActionResult($"General error during file listing: {e.Message}\n" + GetLogs());

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

        private async Task<ActionResult> FileExistsAsync(IAmazonS3 s3Client, string bucketName, ISpecialExecutionTaskTestAction testAction)
        {
            IParameter outputParm = testAction.GetParameter("Output", true);
            IInputValue Ioutput = outputParm.GetAsInputValue();

            try
            {
                IInputValue Is3_FilePath = testAction.GetParameterAsInputValue("S3_FilePath", true);
                IInputValue Is3_FileName = testAction.GetParameterAsInputValue("S3_FileName", true);

                if (Is3_FilePath == null || Is3_FileName == null || Ioutput == null)
                {
                    string missingParamsMessage = $"{(Is3_FilePath == null ? "S3_FilePath, " : "")}" +
                                                  $"{(Is3_FileName == null ? "S3_FileName, " : "")}" +
                                                  $"{(Ioutput == null ? "Output" : "")}".TrimEnd(',', ' ');
                    Log($"The following required parameters are missing: {missingParamsMessage}.");
                    return new UnknownFailedActionResult($"The following required parameters are missing: {missingParamsMessage}.");
                }

                string s3FilePath = Is3_FilePath.Value.Trim();
                string s3FileName = Is3_FileName.Value.Trim();
                string s3Key = $"{s3FilePath}/{s3FileName}";

                Log($"Checking existence of file with key: {s3Key} in S3 bucket: {bucketName}");

                var request = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = s3Key
                };
                var response = await s3Client.GetObjectMetadataAsync(request);
                Log($"File exists. Last modified: {response.LastModified}");
                if (outputParm.ActionMode == ActionMode.Buffer)
                {
                    Buffers.Instance.SetBuffer(Ioutput.Value, "True", false);
                    testAction.SetResultForParameter(outputParm, SpecialExecutionTaskResultState.Ok, $"Buffer {Ioutput.Value} set to True.");
                    return new PassedActionResult($"File exists: {s3Key}\n" + GetLogs());
                }
                else
                {
                    HandleActualValue(testAction, outputParm, Buffers.Instance.GetBuffer(outputParm.Name));
                    return new PassedActionResult($"File exists: {s3Key}\n" + GetLogs());
                }
            }
            catch (AmazonS3Exception e)
            {
                    Log($"File does not exist.");
                    // Set the output buffer to False
                    Buffers.Instance.SetBuffer(Ioutput.Value, "False", false);
                    testAction.SetResultForParameter(outputParm, SpecialExecutionTaskResultState.Ok, $"Buffer {Ioutput.Value} set to False.");
                    return new PassedActionResult($"File does not exist" + GetLogs());
            }
            catch (Exception e)
            {
                Log($"General error during file existence check: {e.Message}");
                return new UnknownFailedActionResult($"General error during file existence check: {e.Message}\n" + GetLogs());
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
