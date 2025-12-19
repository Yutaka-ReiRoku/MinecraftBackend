// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    /* ==========================================
       1. XỬ LÝ NHẠC NỀN (BGM) LIÊN TỤC
       ========================================== */
    const bgm = document.getElementById('bgmAudio');
    const musicBtn = document.getElementById('musicToggleBtn');
    
    // Key để lưu trữ trong LocalStorage
    const STORAGE_KEY_TIME = 'mc_bgm_time';
    const STORAGE_KEY_PLAYING = 'mc_bgm_playing';

    // Hàm cập nhật giao diện nút bấm
    function updateButtonVisual(isPlaying) {
        if (isPlaying) {
            musicBtn.innerHTML = "🔊 Music On";
            musicBtn.classList.remove('btn-outline-secondary');
            musicBtn.classList.add('btn-success');
        } else {
            musicBtn.innerHTML = "🔇 Music Off";
            musicBtn.classList.remove('btn-success');
            musicBtn.classList.add('btn-outline-secondary');
        }
    }

    // Khôi phục trạng thái nhạc khi tải trang
    const savedTime = localStorage.getItem(STORAGE_KEY_TIME);
    const shouldPlay = localStorage.getItem(STORAGE_KEY_PLAYING) === 'true';

    if (savedTime) {
        bgm.currentTime = parseFloat(savedTime);
    }

    // Cố gắng phát nhạc nếu trạng thái cũ là đang phát
    if (shouldPlay) {
        // Lưu ý: Trình duyệt có thể chặn autoplay nếu chưa có tương tác người dùng
        let playPromise = bgm.play();
        if (playPromise !== undefined) {
            playPromise.then(_ => {
                updateButtonVisual(true);
            }).catch(error => {
                console.log("Autoplay bị chặn, cần tương tác người dùng để phát nhạc.");
                updateButtonVisual(false); // Chuyển về tắt nếu bị chặn
            });
        }
    } else {
        updateButtonVisual(false);
    }

    // Sự kiện click nút bật/tắt nhạc
    musicBtn.addEventListener('click', function () {
        if (bgm.paused) {
            bgm.play();
            localStorage.setItem(STORAGE_KEY_PLAYING, 'true');
            updateButtonVisual(true);
        } else {
            bgm.pause();
            localStorage.setItem(STORAGE_KEY_PLAYING, 'false');
            updateButtonVisual(false);
        }
    });

    // Lưu vị trí nhạc liên tục mỗi khi chuyển trang (beforeunload)
    window.addEventListener('beforeunload', function () {
        localStorage.setItem(STORAGE_KEY_TIME, bgm.currentTime);
        // Lưu trạng thái play/pause hiện tại
        localStorage.setItem(STORAGE_KEY_PLAYING, !bgm.paused);
    });

    /* ==========================================
       2. XỬ LÝ ÂM THANH CLICK (GLOBAL)
       ========================================== */
    const clickSound = document.getElementById('clickAudio');

    // Hàm phát tiếng click
    function playClickSound() {
        // Reset về 0 để có thể click liên tục nhanh chóng
        clickSound.currentTime = 0;
        clickSound.play().catch(e => console.error("Click sound error:", e));
    }

    // Tự động gắn tiếng click cho tất cả thẻ a, button, và class .btn
    $(document).on('click', 'a, button, .btn, input[type="submit"], input[type="button"]', function (e) {
        // Không phát tiếng nếu nút đó bị disable hoặc là nút Music (tránh lặp âm)
        if ($(this).prop('disabled') || this.id === 'musicToggleBtn') return;
        
        playClickSound();
    });
});