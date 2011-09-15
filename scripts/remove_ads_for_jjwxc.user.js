// ==UserScript==
// @name jjwxc ads Remover
// @namespace https://code.google.com/p/adblock-chinalist/
// @author Gythialy
// @version 1.0.1
// @description Remove jjwxc.net ads for ChinaList
// @homepage https://code.google.com/p/adblock-chinalist/
// @updateURL https://adblock-chinalist.googlecode.com/svn/trunk/scripts/remove_ads_for_jjwxc.user.js
// @include http://www.jjwxc.net/*
// ==/UserScript==

(function() {
	var DEBUG = 0;
	function log(message) {
		if (DEBUG && GM_log) {
			GM_log(message);
		}
	}

	function remove(elm) {
		if (elm.snapshotItem) {
			for ( var i = 0; i < elm.snapshotLength; i++) {
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

	function x(xpath, parent, type, result) {
		return document.evaluate(xpath, parent || document, null, type || 7, result);
	}

	function $(select) {
		var name = select.substring(1);
		switch (select.charAt(0)) {
		case '#':
			return document.getElementById(name);
		case '.':
			return document.getElementsByClassName(name);
		case '/':
			return document.getElementsByTagName(name);
		default:
			return document.getElementsByName(select);
		}
	}

	function setQCookie(name, value, expire) {
		var crazy_exp = new Date();
		crazy_exp.setTime(crazy_exp.getTime() + expire * 24 * 60 * 60 * 1000);
		document.cookie = name + "=" + value + ";expires=" + crazy_exp.toGMTString();
	}

	function getQCookie(name) {
		var arr = document.cookie.match(new RegExp("(^| )" + name + "=([^;]*)(;|$)"));
		if (arr != null)
			return unescape(arr[2]);

		return null;
	}

	var pop = getQCookie('jj_pop_hy');
	if (pop == null)
		setQCookie('jj_pop_hy', 'popup', 1);

	var script = x('//script[@event="onunload"]');
	if (script) {
		log(script);
		remove(script);
	}

	var form = x('//*[@id="pop_ads"]');
	if (form) {
		log(form);
		remove(form);
	}

	var body = $('/body')[0];
	if (body.hasAttribute('onclick')) {
		body.removeAttribute('onclick');
	}
})();