

function RegisterClient() {
    $(document).ready(function () {
        var serviceURL = '/Register/CheckIfClientExists';

        var username = document.getElementById('txtUsername').value;

        $.ajax({
            type: "GET",
            url: serviceURL,
            data: {username:username},
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc
        });

        function successFunc(data, status) {
            if (data == true) {
                alert('Client username already exists.');
                return false;
            }
            else {
                document.getElementById('registerClient').submit();
            }
        }

        function errorFunc() {
            alert('error occurred. Please try again.');
        }
    });
}



