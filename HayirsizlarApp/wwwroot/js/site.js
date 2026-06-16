// Site JavaScript - Hayırsızlar
document.addEventListener("DOMContentLoaded", () => {
    // 1. File Upload Preview Logic
    const fileInput = document.getElementById("MediaFile");
    const filePreview = document.getElementById("mediaPreviewContainer");
    const previewImage = document.getElementById("previewImage");
    const previewVideo = document.getElementById("previewVideo");
    const removePreviewBtn = document.getElementById("removePreviewBtn");

    if (fileInput && filePreview) {
        fileInput.addEventListener("change", (e) => {
            const file = e.target.files[0];
            if (!file) {
                clearPreview();
                return;
            }

            const fileReader = new FileReader();
            const fileType = file.type;

            fileReader.onload = (event) => {
                filePreview.style.display = "block";

                if (fileType.startsWith("image/")) {
                    previewImage.src = event.target.result;
                    previewImage.style.display = "inline-block";
                    previewVideo.style.display = "none";
                    previewVideo.src = "";
                } else if (fileType.startsWith("video/")) {
                    previewVideo.src = event.target.result;
                    previewVideo.style.display = "inline-block";
                    previewImage.style.display = "none";
                    previewImage.src = "";
                } else {
                    // Fallback for unsupported formats
                    clearPreview();
                    alert("Yalnızca resim veya video yükleyebilirsiniz.");
                }
            };

            fileReader.readAsDataURL(file);
        });

        if (removePreviewBtn) {
            removePreviewBtn.addEventListener("click", (e) => {
                e.preventDefault();
                clearPreview();
            });
        }
    }

    function clearPreview() {
        if (fileInput) fileInput.value = "";
        if (filePreview) filePreview.style.display = "none";
        if (previewImage) {
            previewImage.src = "";
            previewImage.style.display = "none";
        }
        if (previewVideo) {
            previewVideo.src = "";
            previewVideo.style.display = "none";
        }
    }

    // 2. Brutalist Color Picker Logic (For Register and Profile pages)
    const colorPickerContainer = document.querySelector(".color-picker-container");
    const avatarColorInput = document.getElementById("AvatarColor");

    if (colorPickerContainer && avatarColorInput) {
        const colorOptions = colorPickerContainer.querySelectorAll(".color-option");

        colorOptions.forEach(option => {
            // Check if this option matches the currently set color value
            const colorVal = option.getAttribute("data-color");
            if (avatarColorInput.value.toLowerCase() === colorVal.toLowerCase()) {
                option.classList.add("selected");
            }

            option.addEventListener("click", () => {
                // Remove selected class from all
                colorOptions.forEach(opt => opt.classList.remove("selected"));

                // Select current
                option.classList.add("selected");

                // Update input
                avatarColorInput.value = colorVal;
            });
        });
    }

    // 3. Dynamic Reply File Upload Preview
    document.addEventListener("change", (e) => {
        if (e.target.classList.contains("reply-media-input")) {
            const fileInput = e.target;
            const formGroup = fileInput.closest(".form-group");
            const filePreview = formGroup.querySelector(".reply-file-preview");
            const previewImage = formGroup.querySelector(".reply-preview-image");
            const previewVideo = formGroup.querySelector(".reply-preview-video");
            
            const file = fileInput.files[0];
            if (!file) {
                clearReplyPreview(formGroup);
                return;
            }

            const fileReader = new FileReader();
            const fileType = file.type;

            fileReader.onload = (event) => {
                filePreview.style.display = "block";

                if (fileType.startsWith("image/")) {
                    previewImage.src = event.target.result;
                    previewImage.style.display = "inline-block";
                    previewVideo.style.display = "none";
                    previewVideo.src = "";
                } else if (fileType.startsWith("video/")) {
                    previewVideo.src = event.target.result;
                    previewVideo.style.display = "inline-block";
                    previewImage.style.display = "none";
                    previewImage.src = "";
                } else {
                    clearReplyPreview(formGroup);
                    alert("Yalnızca resim veya video yükleyebilirsiniz.");
                }
            };

            fileReader.readAsDataURL(file);
        }
    });

    document.addEventListener("click", (e) => {
        if (e.target.classList.contains("reply-file-preview-remove")) {
            e.preventDefault();
            const formGroup = e.target.closest(".form-group");
            clearReplyPreview(formGroup);
        }
    });

    function clearReplyPreview(formGroup) {
        const fileInput = formGroup.querySelector(".reply-media-input");
        const filePreview = formGroup.querySelector(".reply-file-preview");
        const previewImage = formGroup.querySelector(".reply-preview-image");
        const previewVideo = formGroup.querySelector(".reply-preview-video");

        if (fileInput) fileInput.value = "";
        if (filePreview) filePreview.style.display = "none";
        if (previewImage) {
            previewImage.src = "";
            previewImage.style.display = "none";
        }
        if (previewVideo) {
            previewVideo.src = "";
            previewVideo.style.display = "none";
        }
    }

    // 4. Form empty validation
    document.addEventListener("submit", (e) => {
        const form = e.target;
        if (form.action && (form.action.includes("PostTweet") || form.getAttribute("action") === "/Home/PostTweet")) {
            const textarea = form.querySelector("textarea[name='NewTweet.Content']");
            const fileInput = form.querySelector("input[type='file']");
            
            const content = textarea ? textarea.value.trim() : "";
            const hasFile = fileInput && fileInput.files && fileInput.files.length > 0;
            
            if (content === "" && !hasFile) {
                e.preventDefault();
                alert("Paylaşım yapabilmek için metin yazmalı veya bir resim/video eklemelisiniz.");
            }
        }
    });

    // 5. Mention Autocomplete (WhatsApp Style)
    function initMentionAutocomplete() {
        const initializeTextarea = (textarea) => {
            if (textarea.dataset.mentionInitialized) return;
            textarea.dataset.mentionInitialized = "true";
            setupAutocompleteForTextarea(textarea);
        };

        // Initialize all textareas currently in the DOM
        const textareas = document.querySelectorAll("textarea#NewTweet_Content, textarea.reply-textarea, textarea[name='NewTweet.Content'], textarea[name='content']");
        textareas.forEach(initializeTextarea);

        // Listen for focus on target textareas (for dynamically added or hidden ones)
        document.addEventListener("focusin", (e) => {
            if (e.target.tagName === "TEXTAREA" && (
                e.target.id === "NewTweet_Content" || 
                e.target.classList.contains("reply-textarea") || 
                e.target.name === "NewTweet.Content" || 
                e.target.name === "content"
            )) {
                const textarea = e.target;
                if (!textarea.dataset.mentionInitialized) {
                    initializeTextarea(textarea);
                    // Wrapping causes a temporary DOM removal, so refocus immediately
                    textarea.focus();
                }
            }
        });
    }

    function setupAutocompleteForTextarea(textarea) {
        // Create dropdown list container
        const dropdown = document.createElement("div");
        dropdown.className = "mention-dropdown";
        dropdown.style.display = "none";
        
        // Relative container to position dropdown relative to textarea
        const wrapper = document.createElement("div");
        wrapper.className = "textarea-mention-wrapper";
        wrapper.style.position = "relative";
        
        // Place wrapper in DOM
        textarea.parentNode.insertBefore(wrapper, textarea);
        wrapper.appendChild(textarea);
        wrapper.appendChild(dropdown);

        let activeIndex = 0;
        let currentQuery = "";
        let matchIndex = -1;

        const updateDropdown = (filteredUsers) => {
            dropdown.innerHTML = "";
            if (filteredUsers.length === 0) {
                dropdown.style.display = "none";
                return;
            }

            filteredUsers.forEach((user, idx) => {
                const item = document.createElement("div");
                item.className = "mention-item" + (idx === activeIndex ? " active" : "");
                
                const avatar = document.createElement("div");
                avatar.className = "mention-avatar";
                avatar.style.backgroundColor = user.avatarColor || "#FFE600";
                avatar.textContent = (user.displayName || "??").substring(0, 2).toUpperCase();

                const names = document.createElement("div");
                names.className = "mention-names";

                const dName = document.createElement("span");
                dName.className = "mention-displayname";
                dName.textContent = user.displayName;

                const uName = document.createElement("span");
                uName.className = "mention-username";
                uName.textContent = " @" + user.username;

                names.appendChild(dName);
                names.appendChild(uName);
                item.appendChild(avatar);
                item.appendChild(names);

                item.addEventListener("mousedown", (e) => {
                    e.preventDefault(); // Prevent blur event from firing
                    selectUser(user);
                });

                dropdown.appendChild(item);
            });

            dropdown.style.display = "block";
        };

        const selectUser = (user) => {
            const text = textarea.value;
            const before = text.substring(0, matchIndex);
            
            // Insert username tag
            textarea.value = before + "@" + user.username + " ";
            dropdown.style.display = "none";
            
            textarea.focus();
            const newCursorPos = (before + "@" + user.username + " ").length;
            textarea.setSelectionRange(newCursorPos, newCursorPos);
        };

        textarea.addEventListener("input", () => {
            const users = window.siteUsers || [];
            if (users.length === 0) return;

            const text = textarea.value;
            const cursorPos = textarea.selectionStart;
            const textBeforeCursor = text.substring(0, cursorPos);
            
            // Check if user is typing a mention word (starts with @)
            const match = textBeforeCursor.match(/\B@([a-zA-Z0-9_]*)$/);
            
            if (match) {
                currentQuery = match[1].toLowerCase();
                matchIndex = textBeforeCursor.length - match[0].length;
                
                const filtered = users.filter(u => 
                    u.username.toLowerCase().includes(currentQuery) || 
                    u.displayName.toLowerCase().includes(currentQuery)
                );

                if (activeIndex >= filtered.length) {
                    activeIndex = 0;
                }
                
                updateDropdown(filtered);
            } else {
                dropdown.style.display = "none";
            }
        });

        textarea.addEventListener("keydown", (e) => {
            if (dropdown.style.display === "block") {
                const users = window.siteUsers || [];
                if (users.length === 0) return;

                const text = textarea.value;
                const cursorPos = textarea.selectionStart;
                const textBeforeCursor = text.substring(0, cursorPos);
                const match = textBeforeCursor.match(/\B@([a-zA-Z0-9_]*)$/);
                
                if (!match) return;
                
                const filtered = users.filter(u => 
                    u.username.toLowerCase().includes(currentQuery) || 
                    u.displayName.toLowerCase().includes(currentQuery)
                );

                if (e.key === "ArrowDown") {
                    e.preventDefault();
                    activeIndex = (activeIndex + 1) % filtered.length;
                    updateDropdown(filtered);
                } else if (e.key === "ArrowUp") {
                    e.preventDefault();
                    activeIndex = (activeIndex - 1 + filtered.length) % filtered.length;
                    updateDropdown(filtered);
                } else if (e.key === "Enter") {
                    e.preventDefault();
                    if (filtered[activeIndex]) {
                        selectUser(filtered[activeIndex]);
                    }
                } else if (e.key === "Escape") {
                    e.preventDefault();
                    dropdown.style.display = "none";
                }
            }
        });

        textarea.addEventListener("blur", () => {
            setTimeout(() => {
                dropdown.style.display = "none";
            }, 200);
        });
    }

    // 6. Like/Dislike AJAX Handling
    document.addEventListener("submit", (e) => {
        const form = e.target;
        const actionAttr = form.getAttribute("action") || "";
        if (actionAttr.includes("React") || form.action.includes("/Home/React")) {
            e.preventDefault();
            
            const formData = new FormData(form);
            
            fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Update the UI counts dynamically
                    const container = form.closest("div");
                    if (container) {
                        const forms = container.querySelectorAll("form");
                        forms.forEach(f => {
                            const isLikeInput = f.querySelector("input[name='isLike']");
                            if (isLikeInput) {
                                const isLike = isLikeInput.value === "true";
                                const button = f.querySelector("button");
                                if (button) {
                                    if (isLike) {
                                        button.innerHTML = `👍 ${data.likes}`;
                                        button.className = `nav-btn ${data.hasLiked ? 'btn-yellow' : 'btn-white'}`;
                                    } else {
                                        button.innerHTML = `👎 ${data.dislikes}`;
                                        button.className = `nav-btn ${data.hasDisliked ? 'btn-yellow' : 'btn-white'}`;
                                    }
                                }
                            }
                        });
                    }
                }
            })
            .catch(err => {
                console.error("Reaction error:", err);
                form.submit(); // fallback
            });
        }
    });

    initMentionAutocomplete();
});
