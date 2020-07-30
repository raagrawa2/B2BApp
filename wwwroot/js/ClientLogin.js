
function LoginClient() {
    $(document).ready(function () {
        var serviceURL = '/Login/Loginclient';

        var username = document.getElementById('txtUserName').value;
        var password = document.getElementById('txtPassword').value;

        if (username.trim() == '') {
            alert('Please enter username.');
            return false;
        }

        if (password.trim() == '') {
            alert('Please enter password.');
            return false;
        }

        var model = {};
        model.UserName = username;
        model.Password = password;

        $.ajax({
            type: "POST",
            url: serviceURL,
            data: /*'{ dt:' + */JSON.stringify(model) /*+ '}'*/,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc
        });

        function successFunc(data, status) {
            if (data == "Client logged in successfully.") {
                //alert('Client logged in..');
                window.location.href = "/home/LoggedInUser";
                return true;
            }
            else {
                alert(data);
                return false;
            }
        }

        function errorFunc() {
            alert('error occurred. Please try again.');
        }
    });
}
