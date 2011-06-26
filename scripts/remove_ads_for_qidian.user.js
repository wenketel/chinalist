// ==UserScript==
// @name Qidian ads Remover
// @author Gythialy
// @run-at document-start
// @description Remove qidian.com ads for ChinaList (Only for Scriptish)
// @create 2011-6-17
// @lastmodified 2011-6-26
// @version 1.0.1
// @namespace http://code.google.com/p/adblock-chinalist/
// @include http://www.qidian.com/*
// ==/UserScript==
(function() {
	document.addEventListener("beforescriptexecute", function(e) {
				if (e.target.innerHTML.indexOf('SNDABackPop') > -1) {
					e.stopPropagation();
					e.preventDefault();
					// alert(e.target.innerHTML + ' stoped.');
				}
			}, true);

	document.addEventListener("DOMNodeInserted", function(e) {
				function x(xpath, parent, type, result) {
					return document.evaluate(xpath, parent || document, null, type || 7, result);
				}

				function remove(elm) {
					if (elm.snapshotItem) {
						for (var i = 0; i < elm.snapshotLength; i++) {
							remove(elm.snapshotItem(i));
						}
					} else if (elm[0]) {
						while (elm[0]) {
							remove(elm[0]);
						}
					} else {
						elm.parentNode.removeChild(elm);
					}
				}

				var scripts = x('//script');
				for (var index = 0; index < scripts.snapshotLength; index++) {
					var script = scripts.snapshotItem(index);
					var t = new String(script.innerHTML);
					if (t.indexOf('SNDABackPop') != -1 || t.indexOf('eAddMark') != -1 || t.indexOf('SNDAADAltern') != -1) {
						// alert('remove ' + t);
						remove(script);
					}
				}
			}, false);
})();