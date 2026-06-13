document.addEventListener("DOMContentLoaded", () => {
    // Initialize all retro TV players on the page
    const players = document.querySelectorAll(".retro-tv-player");

    players.forEach(player => {
        const video = player.querySelector("video");
        const playOverlay = player.querySelector(".video-play-overlay");
        const playBtn = player.querySelector(".btn-play-toggle");
        const timelineInput = player.querySelector(".timeline-input");
        const timelineFill = player.querySelector(".timeline-fill");
        const timeCounter = player.querySelector(".time-counter");
        const volumeSlider = player.querySelector(".volume-slider");
        const muteBtn = player.querySelector(".btn-mute-toggle");

        if (!video) return;

        // Formats time in MM:SS
        const formatTime = (seconds) => {
            if (isNaN(seconds)) return "00:00";
            const mins = Math.floor(seconds / 60);
            const secs = Math.floor(seconds % 60);
            return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
        };

        // Update time display and timeline track
        const updateTimeline = () => {
            if (video.duration) {
                const percentage = (video.currentTime / video.duration) * 100;
                timelineFill.style.width = `${percentage}%`;
                timelineInput.value = percentage;
                timeCounter.textContent = `${formatTime(video.currentTime)} / ${formatTime(video.duration)}`;
            }
        };

        // When video metadata is loaded
        video.addEventListener("loadedmetadata", () => {
            timeCounter.textContent = `00:00 / ${formatTime(video.duration)}`;
        });

        // Toggle Play/Pause
        const togglePlay = () => {
            if (video.paused) {
                // Pause all other videos first to prevent multiple overlays playing audio at once
                document.querySelectorAll(".retro-tv-player video").forEach(otherVideo => {
                    if (otherVideo !== video) {
                        otherVideo.pause();
                    }
                });
                video.play();
            } else {
                video.pause();
            }
        };

        // Handle play and pause visual states
        video.addEventListener("play", () => {
            if (playOverlay) playOverlay.style.display = "none";
            if (playBtn) playBtn.textContent = "||"; // Pause sign
        });

        video.addEventListener("pause", () => {
            if (playOverlay) playOverlay.style.display = "flex";
            if (playBtn) playBtn.textContent = "▶"; // Play sign
        });

        video.addEventListener("timeupdate", updateTimeline);

        // Click listeners
        if (playOverlay) playOverlay.addEventListener("click", togglePlay);
        if (playBtn) playBtn.addEventListener("click", togglePlay);
        video.addEventListener("click", togglePlay);

        // Timeline scrubbing
        timelineInput.addEventListener("input", (e) => {
            const percentage = e.target.value;
            timelineFill.style.width = `${percentage}%`;
            if (video.duration) {
                video.currentTime = (percentage / 100) * video.duration;
            }
        });

        // Volume control
        if (volumeSlider) {
            volumeSlider.addEventListener("input", (e) => {
                video.volume = e.target.value;
                video.muted = (video.volume === 0);
                if (muteBtn) {
                    muteBtn.textContent = video.muted ? "🔇" : "🔊";
                }
            });
        }

        // Mute button toggle
        if (muteBtn) {
            muteBtn.addEventListener("click", () => {
                video.muted = !video.muted;
                muteBtn.textContent = video.muted ? "🔇" : "🔊";
                if (volumeSlider) {
                    volumeSlider.value = video.muted ? 0 : video.volume;
                }
            });
        }
    });
});
