showElement = (elementId) => { $(`#${elementId}`).show(); };

hideElement = (elementId) => { $(`#${elementId}`).hide(); };

enable = (elementId) => { $(`#${elementId}`).prop('disabled', false); }

disable = (elementId) => { $(`#${elementId}`).prop('disabled', true); }

addClass = (elementId, theClass) => { $(`#${elementId}`).addClass(theClass); }

removeClass = (elementId, theClass) => { $(`#${elementId}`).removeClass(theClass); }

setText = (elementId, text) => { $(`#${elementId}`).text(text); }

setValue = (elementId, value) => { $(`#${elementId}`).val(value); };

isValidInput = (elementId) => { return $(`#${elementId}`)[0].validity.valid; }

removeText = (elementId) => { $(`#${elementId}`).text(null); }

setTitle = (title) => { $(document).prop('title', title); };

setZIndex = (elementId, value) => { $(`#${elementId}`).css('z-index', value); };
