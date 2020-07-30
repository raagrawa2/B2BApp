// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function showRegisterDiv() {

    document.getElementById('divRegister').style.display = 'inline-flex';
    document.getElementById('divLogin').style.display = 'none';
    return true;
}


function showLoginDiv() {

    document.getElementById('divRegister').style.display = 'none';
    document.getElementById('divLogin').style.display = 'inline-flex';
    return true;
}




