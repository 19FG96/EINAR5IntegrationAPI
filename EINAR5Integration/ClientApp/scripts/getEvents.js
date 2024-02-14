document.getElementById('eventForm').addEventListener('submit', function (event) {
    event.preventDefault();

    var startTime = document.getElementById('startDate').value;
    var endTime = document.getElementById('endDate').value;

    // Check if start time and end time are not null
    if (!startTime || !endTime) {
        alert('Start time and end time must be provided.');
        return;
    }

    var data = {
        startTime: startTime,
        endTime: endTime
    };

    // Show the loading modal
    document.getElementById('loadingModal').style.display = 'block';
    document.getElementById('loadingText').style.display = 'block';

    fetch('https://localhost:7000/api/Device/GetEvents', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
})
.then(response => response.json())
.then(data => {
    // Hide the loading modal
    document.getElementById('loadingModal').style.display = 'none';
    document.getElementById('loadingText').style.display = 'none';

    var eventList = document.getElementById('eventList');
    eventList.innerHTML = '';

    if (Array.isArray(data) && data.length === 0) {
        // If no events were found, display a message
        eventList.textContent = 'No events found for the given time period.';
    } else if (Array.isArray(data)) {
        // If events were found, display them
        data.forEach(function (storageEvent) {
            var eventCard = document.createElement('div');
            eventCard.className = 'event-card';
        
            var image = storageEvent.eventImage;
            var imageElement = image ? `<img class="event-image" src="data:image/jpeg;base64,${image.data}" width="50px;" height="auto"/>` : 'No image';

            var video = storageEvent.eventVideo;
            var videoElement = video ? `<video controls><source src="data:video/mp4;base64,${video.data}" type="video/mp4"></video>` : 'No video';

            var html = `
                    <h2>${storageEvent.device.name}</h2>
                    <p>DetectorVersion: ${storageEvent.detectorVersion}</p>
                    <p>DetectorID: ${storageEvent.detectorID}</p>
                    <p>DetectorClassID: ${storageEvent.detectorClassID}</p>
                    <p>EventTime: ${storageEvent.eventTime}</p>
                    <p>EventTriggerTime: ${storageEvent.eventTriggerTime}</p>
                    <p>State: ${storageEvent.state}</p>
                    <p>EventCode: ${storageEvent.eventCode}</p>
                    <p>EventID: ${storageEvent.eventID}</p>
                    <p>DetectorEventType: ${storageEvent.detectorEventType}</p>
                    <details>
                        <summary>Config</summary>
                    <p>Config: ${JSON.stringify(storageEvent.config)}</p>
                    </details>
                    <p>Device: ${JSON.stringify(storageEvent.device)}</p>
                    <details>
                        <summary>SourceData</summary>
                        <p>${JSON.stringify(storageEvent.sourceData)}</p>
                    </details>
                    <p>EventImage: ${imageElement}</p>
                    <p>EventVideo: ${videoElement}</p>
                `;
        
            eventCard.innerHTML = html;
            eventList.appendChild(eventCard);

            // image modal
            eventCard.querySelector('.event-image').addEventListener('click', function() {
                var modal = document.createElement('div');
                modal.className = 'modal';
                modal.innerHTML = `<img src="${this.src}" style="max-width: 90%; max-height: 90%;"/>`;
                modal.addEventListener('click', function() {
                    document.body.removeChild(this);
                });
                document.body.appendChild(modal);
            });
        });
    } else if (data.message) {
        // If the server returned an object with a message property, display the message
        eventList.textContent = data.message;
    }
})
.catch((error) => {
    console.error('Error:', error);
});
});


