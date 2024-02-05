let eye = false;
function onpress() {
    if (!eye) {
        document.getElementsByClassName('person-icon')[2].classList.add('hide-eye');
        document.getElementsByClassName('person-icon')[1].classList.remove('hide-eye');
        document.getElementById('floatingPassword').type = "text";
        eye = true;
    } else {
        document.getElementsByClassName('person-icon')[2].classList.remove('hide-eye');
        document.getElementsByClassName('person-icon')[1].classList.add('hide-eye');
        document.getElementById('floatingPassword').type = "password";
        eye = false;
    }
}

window.onload = function checkingTheme() {
    let themeCheck = localStorage.getItem('theme');
    if (themeCheck == null || themeCheck == 'light') {
        document.getElementById('main-page').style.backgroundColor = '#fcfcfc';
        document.getElementById('login-text').style.color = 'black';
        document.getElementsByClassName('first-input')[0].style.backgroundColor = 'white';
        document.getElementsByClassName('first-input')[1].style.backgroundColor = 'white';
        document.getElementById('term').style.color = 'gray';
        document.getElementById('of').style.color = 'gray';
        document.getElementById('privacy').style.color = 'gray';
        document.getElementsByClassName('change-theme')[1].classList.add('hide-icon');
        document.getElementsByClassName('change-theme')[0].classList.remove('hide-icon');
    }
    else {
        document.getElementById('main-page').style.backgroundColor = 'black';
        document.getElementById('login-text').style.color = 'white';
        document.getElementsByClassName('first-input')[0].style.backgroundColor = 'white';
        document.getElementsByClassName('first-input')[1].style.backgroundColor = 'white';
        document.getElementById('term').style.color = 'white';
        document.getElementById('of').style.color = 'white';
        document.getElementById('privacy').style.color = 'white';
        document.getElementsByClassName('change-theme')[0].classList.add('hide-icon');
        document.getElementsByClassName('change-theme')[1].classList.remove('hide-icon');
    }
}

function changeTheme() {
    let theme = localStorage.getItem('theme');
    if (theme == null || theme == 'light') {
        localStorage.setItem('theme', 'dark');
        console.log('theme changed to dark');
        document.getElementById('main-page').style.backgroundColor = 'black';
        document.getElementById('login-text').style.color = 'white';
        document.getElementsByClassName('first-input')[0].style.backgroundColor = 'white';
        document.getElementsByClassName('first-input')[1].style.backgroundColor = 'white';
        document.getElementById('term').style.color = 'white';
        document.getElementById('of').style.color = 'white';
        document.getElementById('privacy').style.color = 'white'; 
        document.getElementsByClassName('change-theme')[0].classList.add('hide-icon');
        document.getElementsByClassName('change-theme')[1].classList.remove('hide-icon');
    }
    else if (theme == 'dark') {
        localStorage.setItem('theme', 'light');
        document.getElementById('main-page').style.backgroundColor = '#fcfcfc';
        document.getElementById('login-text').style.color = 'black';
        document.getElementsByClassName('first-input')[0].style.backgroundColor = 'white';
        document.getElementsByClassName('first-input')[1].style.backgroundColor = 'white';
        document.getElementById('term').style.color = 'gray';
        document.getElementById('of').style.color = 'gray';
        document.getElementById('privacy').style.color = 'gray';
        document.getElementsByClassName('change-theme')[1].classList.add('hide-icon');
        document.getElementsByClassName('change-theme')[0].classList.remove('hide-icon');
    }
}