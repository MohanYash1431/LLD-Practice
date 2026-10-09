**Functional Requirements:**

1. Accept Notification Request.
2. Support Different Channels. Ex: Email, SMS, Push.
3. Respect User Preferences.
4. Support Asynchronus Submission.
5. Format messages per channel.
6. Retry Temporary Failures.
7. Rate Limitting.
8. Track Notification Status.


**Non-functional Requirements**

1. Thread Safety : Support Concurrent notification submission safely.
2. Responsiveness: Process notifications Asynchronusly.
3. Extensiblity: Easy to add new channels.
4. Maintanablity: Easy to extend/new Implementation using interfaces.

**Identifying Core API's/Functions and Entities**

Functions/API's

NotificationService
1. POST/SubmitNotificationAsync(UserId, message, recipient information) : Accepts Notification for processing and returns Id.
     Entities: Notification, Recipient Information
   
3. GetStatus(NotificationId) : Return Status of each channel for given Notificatioon Id.
   Enttities: Per Channel Statuses.

UserPreferenceService   
4. GET/GetUserPreferences(UserId) : Returns User Enabled Channels.

NotificationDispatcherService
5. SubmitAsync(Notification, Enabled Channels) : Dispatcher Operation - Queue the work for Background processing.
   Entities: Notification, enabled channel types, background work

NotificationFactory
6. GetChannel(Channel Type Ex: Email, SMS and Push) : Returns corresponding channel Implementations.
 Entities : NotificationType and registered INotificationChannel implementations

 INotificationChannel
7. SendAsync(Notification info for channel) : Complete provider call Successfully or Report failure.
   Enties : Information required to send through one channel

Email/SMS/Push channel classes
   Implement SendAsync : Message and the corresponding email address, phone number or push token
8. UpateStatus(NotificationId, Channel amd New Status) : Record channels progress or outcome.

NotificationStatusStore
UpdateStatus(notificationId, channel, newStatus); GetStatus(notificationId
 Enties : Notification ID, NotificationType, NotificationStatus

