setTitle = (title) => { document.title = title; };
hideElement = (elementId) => { setElementStyleDisplay(elementId, "none"); };
showElement = (elementId) => { setElementStyleDisplay(elementId, "block"); };
setElementStyleDisplay = (elementId, styleDisplay) => { document.getElementById(elementId).style.display = styleDisplay; };
