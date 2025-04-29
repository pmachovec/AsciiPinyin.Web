showElement = (elementId) => { $(`#${elementId}`).show(); };

hideElement = (elementId) => { $(`#${elementId}`).hide(); };

enable = (elementId) => { $(`#${elementId}`).prop('disabled', false); }

disable = (elementId) => { $(`#${elementId}`).prop('disabled', true); }

getText = (elementId) => { return $(`#${elementId}`).text(); }

setText = (elementId, text) => { $(`#${elementId}`).text(text); }

setValue = (elementId, value) => { $(`#${elementId}`).val(value); };

setAttribute = (elementId, attributeName, value) => { $(`#${elementId}`).attr(attributeName, value); }

isValidInput = (elementId) => { return $(`#${elementId}`)[0].validity.valid; }

removeText = (elementId) => { $(`#${elementId}`).text(null); }

setTitle = (title) => { $(document).prop('title', title); };

setZIndex = (elementId, value) => { $(`#${elementId}`).css('z-index', value); };
