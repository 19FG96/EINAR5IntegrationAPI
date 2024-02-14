document.getElementById('eventForm').addEventListener('submit', function (event) {
    event.preventDefault();

    var username = document.getElementById('username').value;
    var password = document.getElementById('password').value;

    var data = {
        username: username,
        password: password
    };

    fetch('https://localhost:7000/api/Device/Login', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
})
.then(response => {
    if (!response.ok) {
        // Only parse the response as JSON if there's a response body
        return response.text().then(text => {
            try {
                // Try to parse the text as JSON
                const data = JSON.parse(text);
                throw new Error(data.message);
            } catch {
                // If the text is not valid JSON, throw an error
                throw new Error('Login failed.');
            }
        });
    }
    // Only parse the response as JSON if there's a response body
    return response.text().then(text => text ? JSON.parse(text) : {});
})
.then(data => {
    // If login is successful, redirect to a new HTML page
    window.location.href = "eventsPage.html";
})
.catch((error) => {
    // Display the error message to the user
    alert(error.message);
    console.error('Error:', error);
});
});