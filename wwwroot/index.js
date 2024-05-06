function redirectToDeleteApi (event, element) {
    event.preventDefault();

    var cityId = element.getAttribute("data-city-id");

    fetch(`/admin/cities/delete/${cityId}`, { method: "DELETE" })
    .then(response => {
        if (response.ok) {
            updateCitiesAndOptions();
        } else {
            console.error("Failed to delete city:", response.statusText);
        }
    })
    .catch(error => {
        console.error("Error deleting city:", error);
    });
}

function updateCitiesAndOptions() {
    fetch("/api/options", { method: "GET" })
    .then(response => { return response.text(); })
    .then(htmlContent => {
        var options = document.getElementById("options");
        options.innerHTML = htmlContent;
    });
    
    fetch("/api/cities", { method: "GET" })
    .then(response => { return response.text(); })
    .then(htmlContent => {
        var cities = document.getElementById("cities");
        cities.innerHTML = htmlContent;
    });
}

function addCity(event) {
    event.preventDefault();

    const form = document.getElementById("addForm");
    const formData = new FormData(form);

    fetch(form.action, {
        method: form.method,
        body: formData
    })
    .then(() => updateCitiesAndOptions())
    .catch(error => { console.error("Error:", error); });
}