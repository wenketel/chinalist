// ==UserScript==
// @name Youku ads Remover
// @author Gythialy
// @description  Remove youku.com ads for ChinaList
// @create 2011-7-15
// @lastmodified 2011-7-15
// @version 1.0.0
// @namespace  http://code.google.com/p/adblock-chinalist/
// @include http://v.youku.com/v_show/*
// ==/UserScript==

(function() {
    document.addEventListener("DOMNodeInserted", function(e) {
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

        function x(xpath, parent, type, result) {
            return document.evaluate(xpath, parent || document, null, type || XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, result);
        }

        remove(x('//script[contains(@src, ".scorecardresearch.com")]'));
        remove(x('//script[contains(@src, ".atm.youku.com")]'));
        remove(x('//script[contains(@src, ".lstat.youku.com")]'));
        remove(x('//div[starts-with(@id,"ab_")]'));
//        remove(x('//noscript'));

        var scripts = x('//script');
        for (var i = 0,length = scripts.snapshotLength; i < length; i++) {
            var script = scripts.snapshotItem(i);
            var t = script.innerHTML;
            if (t.indexOf('Nova.addScript') > -1 || t.indexOf('COMSCORE.beacon') > -1) {
                remove(script);
            }
        }
    }, false);
})();