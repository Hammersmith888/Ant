let timerIdTouch = setInterval(() => {

    if (document.querySelector('.swipe-fullscreen') !== null) {
        clearInterval(timerIdTouch);
        let div = document.createElement('div');
        div.className = "touch-event-fix";
        div.style = 'position: fixed;width: 100%;height: 100%;z-index: 50000;'
        document.body.append(div);
        div.addEventListener('touchstart', function (e) {
            document.getElementById("wgl_game").removeAttribute('style');
            document.querySelector('.swipe-fullscreen').remove()
            document.querySelector('.touch-event-fix').remove()
        });
    }
}, 1000);