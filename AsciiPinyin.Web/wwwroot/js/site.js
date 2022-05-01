// In Blazor Server application, there's no way how to achieve dynamic title changes without JavaScript.
setTitle = (title) => { document.title = "ASCII Pinyin - " + title; };
