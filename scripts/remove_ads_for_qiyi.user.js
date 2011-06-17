// ==UserScript==
// @name Qiyi ads Remover
// @author Gythialy
// @description  Remove qiyi.com flash ads for ChinaList
// @create 2011-6-16
// @lastmodified 2011-6-17
// @version 1.0.2
// @namespace  http://code.google.com/p/adblock-chinalist/
// @include http://www.qiyi.com/*
// @include http://yule.qiyi.com/*
// ==/UserScript==

(function() {
	function getQCookie(name) {
		var arr = document.cookie.match(new RegExp("(^| )" + name + "=([^;]*)(;|$)"));
		if (arr != null)
			return unescape(arr[2]);

		return null;
	}

	function setQCookie(name, value, expire) {
		var crazy_exp = new Date();
		crazy_exp.setTime(crazy_exp.getTime() + expire * 24 * 60 * 60 * 1000);
		document.cookie = name + "=" + value + ";expires=" + crazy_exp.toGMTString();
	}

	function closeCrazy() {
		var jdtflash = document.getElementById('jdtflash');
		if (jdtflash)
			jdtflash.style.display = '';
		var adflash = document.getElementById('adflash');
		if (adflash)
			adflash.style.display = 'none';
	}

	closeCrazy();
	
	var url = window.location.href.toString();

	var qycrazy = getQCookie('qycrazy');
	qycrazy = qycrazy == null ? 0 : qycrazy;
	if (qycrazy < 2 && url.indexOf('www') != -1) {
		setQCookie('qycrazy', 2, 1);
	}

	var qyylcrazy = getQCookie('qyylcrazy');
	qyylcrazy = qyylcrazy == null ? 0 : qyylcrazy;
	if (qycrazy < 2 && url.indexOf('yule') != -1) {
		setQCookie('qyylcrazy', 2, 1);
	}
})();