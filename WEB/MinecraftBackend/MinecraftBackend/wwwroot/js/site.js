// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
    /* =================================================================
       1. GLOBAL CLICK SOUND (XỬ LÝ TIẾNG CLICK BẤT CỨ ĐÂU)
       ================================================================= */
    const clickSource = document.getElementById('clickAudio');

    // Bắt sự kiện click trên toàn bộ tài liệu (document)
    // Sử dụng 'mousedown' để âm thanh phản hồi nhanh hơn ngay khi nhấn xuống
    $(document).on('mousedown', function (e) {
        // Nếu cloneNode không được hỗ trợ, fallback về play thường
        if (clickSource) {
            // Tạo một bản sao của thẻ audio để có thể phát nhiều tiếng cùng lúc
            // (tránh trường hợp click nhanh quá tiếng cũ bị ngắt)
            const soundClone = clickSource.cloneNode();
            soundClone.volume = 0.6; // Chỉnh âm lượng tiếng click (0.0 đến 1.0)
            soundClone.play().catch(() => { 
                // Bỏ qua lỗi nếu trình duyệt chưa cho phép phát
            });
        }
    });

    /* =================================================================
       2. BACKGROUND MUSIC (BGM) - GIỮ TRẠNG THÁI KHI CHUYỂN TRANG
       ================================================================= */
    const bgm = document.getElementById('bgmAudio');
    const musicBtn = document.getElementById('musicToggleBtn');

    const KEY_TIME = 'mc_bgm_time';
    const KEY_PLAYING = 'mc_bgm_playing';

    // Cấu hình âm lượng nhạc nền
    bgm.volume = 0.4; 

    // Hàm cập nhật giao diện nút
    function updateBtnState(isPlaying) {
        if (isPlaying) {
            musicBtn.innerHTML = "🔊 Music On";
            musicBtn.classList.replace('btn-outline-secondary', 'btn-success');
        } else {
            musicBtn.innerHTML = "🔇 Music Off";
            musicBtn.classList.replace('btn-success', 'btn-outline-secondary');
        }
    }

    // --- LOGIC KHÔI PHỤC NHẠC ---
    const savedTime = localStorage.getItem(KEY_TIME);
    const shouldPlay = localStorage.getItem(KEY_PLAYING) === 'true';

    // Khôi phục vị trí thời gian
    if (savedTime) {
        bgm.currentTime = parseFloat(savedTime);
    }

    if (shouldPlay) {
        // Cố gắng phát nhạc ngay lập tức
        const playPromise = bgm.play();

        if (playPromise !== undefined) {
            playPromise.then(() => {
                // Thành công: nhạc chạy
                updateBtnState(true);
            }).catch(error => {
                // THẤT BẠI: Do trình duyệt chặn Autoplay khi mới load trang
                console.log("Browser blocked autoplay. Waiting for user interaction...");
                updateBtnState(false); // Tạm thời hiển thị tắt
                
                // GIẢI PHÁP: Chờ cú click đầu tiên bất kỳ đâu để kích hoạt lại nhạc
                $(document).one('click', function () {
                    bgm.play().then(() => {
                        updateBtnState(true);
                        localStorage.setItem(KEY_PLAYING, 'true'); // Đảm bảo trạng thái đúng
                    });
                });
            });
        }
    } else {
        updateBtnState(false);
    }

    // --- LOGIC NÚT BẬT/TẮT ---
    musicBtn.addEventListener('click', function (e) {
        // Ngăn sự kiện này lan ra document để không bị kích hoạt tiếng click 2 lần (nếu muốn)
        // e.stopPropagation(); 
        
        if (bgm.paused) {
            bgm.play();
            localStorage.setItem(KEY_PLAYING, 'true');
            updateBtnState(true);
        } else {
            bgm.pause();
            localStorage.setItem(KEY_PLAYING, 'false');
            updateBtnState(false);
        }
    });

    // --- LƯU TRẠNG THÁI KHI RỜI TRANG ---
    window.addEventListener('beforeunload', function () {
        localStorage.setItem(KEY_TIME, bgm.currentTime);
        // Lưu trạng thái thực tế (đang chạy hay đang dừng)
        localStorage.setItem(KEY_PLAYING, !bgm.paused);
    });
});