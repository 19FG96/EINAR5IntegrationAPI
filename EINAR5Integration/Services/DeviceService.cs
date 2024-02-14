using Harbard;
using Harbard.Api;
using EINAR5Integration.models;
using EINAR5Integration.Services.CompressServices;

namespace EINAR5Integration.Services
{
    public class DeviceService
    {
        private readonly ILogger<DeviceService> _logger;

        private static string UserName;
        private static string Password;
        private static string EINAR5Address = "192.168.48.220";


        public DeviceService(ILogger<DeviceService> logger)
        {
            _logger = logger;
        }

        public (bool, string) Login(AuthRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Invalid request body.");
                return (false, "Invalid request body.");
            }

            string deviceAddress = EINAR5Address;
            string username = request.Username;
            string password = request.Password;



            try
            {
                var apiSession = new ApiSession(deviceAddress, username, password);

                if (apiSession != null && apiSession.Session.Id != null)
                {
                    // _httpContextAccessor.HttpContext.Items["Username"] = username;
                    // _httpContextAccessor.HttpContext.Items["Password"] = password;

                    UserName = username;
                    Password = password;

                    // Login succeeded, session is valid
                    _logger.LogInformation("Login successful. Session ID: {SessionId}", apiSession.Session.Id);

                    return (true, null);
                }
                else
                {
                    _logger.LogWarning("Login failed.");

                    if (apiSession?.Session?.LastError != null)
                    {
                        var errorMessage = "Error: " + apiSession.Session.LastError.exceptionClass + " - " + apiSession.Session.LastError.errorMessage;
                        return (false, errorMessage);
                    }
                    return (false, "Login failed.");
                }


            }
            catch (Harbard.ApiException ex)
            {
                if (ex.Message == "The username or password is invalid!")
                {
                    _logger.LogWarning("Invalid credentials. Please try again.");
                    return (false, "Invalid credentials. Please try again.");
                }
                else
                {
                    _logger.LogError(ex, "An unexpected error occurred.");
                    return (false, "An unexpected error occurred. Please try again.");
                }
            }
        }

        public  (bool, List<object>, string) GetEvents(EventRequest request)
        {
            string address = EINAR5Address;
            string username = UserName;
            string password = Password;
            DateTime startTime = request.StartTime;
            DateTime endTime = request.EndTime;

            var events = new List<object>();

            using (var apiSession = new ApiSession(address, username, password, 80))
            {
                if (apiSession)
                {
                    StoredEventQuery storedEventQuery = new StoredEventQuery(apiSession.Storage, startTime, endTime);

                    StorageEvents storageEvents = storedEventQuery.Run();

                    if (storageEvents._EventList != null) //check is for avoiding compile warning only
                    {
                        foreach (var storageEvent in storageEvents._EventList.Take(6))
                        {
                            
                            Console.WriteLine($"{storageEvent._Config._DisplayName} : {storageEvent._EventID} : {storageEvent._EventTime}");

                            
                            var image = storedEventQuery.GetEventImage(storageEvent);


                            // Compress image for better performance
                            if (image != null)
                            {
                                Compressor compressor = new Compressor();
                                compressor.CompressImage(image);
                            }


                            // Convert the image data to a Base64 string
                            string imageBase64String = null;
                            if (image?.data != null)
                            {
                                imageBase64String = Convert.ToBase64String(image.data);
                            }

                           
                            var video = storedEventQuery.GetEventVideo(storageEvent);

                            // Compress video for better performance
                            if (video != null)
                            {
                                Compressor compressor = new Compressor();
                                compressor.CompressVideo(video);
                            }

                            // Convert the video data to a Base64 string
                            string videoBase64String = null;
                            if (video?.data != null)
                            {
                                videoBase64String = Convert.ToBase64String(video.data);
                            }

                            // Add event data to the list
                            events.Add(new
                            {
                                DetectorVersion = storageEvent._DetectorVersion,
                                DetectorID = storageEvent._DetectorID,
                                DetectorClassID = storageEvent._DetectorClassID,
                                EventTime = storageEvent._EventTime,
                                EventTriggerTime = storageEvent._EventTriggerTime,
                                State = storageEvent._State,
                                EventCode = storageEvent._EventCode,
                                EventID = storageEvent._EventID,
                                DetectorEventType = storageEvent._DetectorEventType,
                                Config = storageEvent._Config == null ? null : new
                                {
                                    Class = storageEvent._Config._Class,
                                    Version = storageEvent._Config._Version,
                                    DetectorClassID = storageEvent._Config._DetectorClassID,
                                    BuiltIn = storageEvent._Config._BuiltIn,
                                    DetectorID = storageEvent._Config._DetectorID,
                                    DisplayName = storageEvent._Config._DisplayName,
                                    Description = storageEvent._Config._Description,
                                    ViolationTimeMs = storageEvent._Config._ViolationTimeMs,
                                    RestoreDelayMs = storageEvent._Config._RestoreDelayMs,
                                    FpsLimit = storageEvent._Config._FpsLimit,
                                    Enabled = storageEvent._Config._Enabled
                                },
                                Device = storageEvent._Device == null ? null : new
                                {
                                    Name = storageEvent._Device._Name,
                                    Description = storageEvent._Device._Description,
                                    Serial = storageEvent._Device._Serial,
                                    Location = storageEvent._Device._Location
                                },
                                SourceData = storageEvent._SourceData,
                                EventImage = image == null ? null : new
                                {
                                    Format = image.format,
                                    Data = imageBase64String,
                                    Width = image.width,
                                    Height = image.height,
                                    Index = image.index,
                                    ImageId = image.imageId
                                },
                                EventVideo = video == null ? null : new
                                {
                                    Format = video.format,
                                    Data = videoBase64String
                                }
                            });

                        }
                    }
                }
                else if (apiSession.Session.LastError != null) //LastError check is for avoiding compile warning only
                {
                    Console.Error.WriteLine(apiSession.Session.LastError.exceptionClass + " : " + apiSession.Session.LastError.errorMessage);
                }

                if (events.Count == 0)
                {
                    return (false, null, "No events were found in the specified time range.");
                }
            }

            return (true, events, null);
        }
    }
}
