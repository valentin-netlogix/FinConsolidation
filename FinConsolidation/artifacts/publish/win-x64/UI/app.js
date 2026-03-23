/* ui/app.js */
'use strict';

/* =========================
   Presentation configuration
   ========================= */

// Column alignment (match by SQL name or final header caption)
const columnAlignment = {
  'ShipmentID': 'left',
  'Client': 'left',
  'Consignment': 'left',
  'SRRef': 'left',
  'Date': 'left',
  'Status': 'left',
  'Trip': 'left',
  'Carrier': 'left',
  'VehType': 'center',
  'Qty': 'right',
  'Vol': 'right',
  'Wgt': 'right',
  'Domain': 'left',
  'TVAmt': 'right',
  'SVAmt': 'right',      // AR "TVAmt"
  'Inv': 'center',
  'IsCon': 'center',
  'ConCharge': 'left',
  'ConChCode': 'left',
  'ConAmt': 'right',
  'ConFaf': 'right',
  'ConFafAmt': 'right',
  'ConMod': 'center',
  'UOM': 'center',
  'CalcCharge': 'left',
  'CalcChCode': 'left',
  'CalcAmt': 'right',    // AR "CalcTotAmt"
  'CalcTotAmt': 'right',
  'CalcFaf': 'right',
  'CalcFafAmt': 'right',
  'CalcMod': 'center',
  'ConPass': 'center',
  'CalcPass': 'center',
  'Excl': 'center'
};

// Per-column formatting (match on friendly header OR SQL name)
const columnFormat = {
  'TVAmt':      { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'SVAmt':      { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'ConAmt':     { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'ConFaf':     { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'ConFafAmt':  { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'CalcAmt':    { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'CalcTotAmt': { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'CalcFaf':    { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },
  'CalcFafAmt': { type: 'currency', currency: 'NZD', locale: 'en-NZ', minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: true, accounting: false },

  'Qty': { type: 'number', minimumFractionDigits: 0, maximumFractionDigits: 0, useGrouping: false },
  'Vol': { type: 'number', minimumFractionDigits: 3, maximumFractionDigits: 3, useGrouping: false },
  'Wgt': { type: 'number', minimumFractionDigits: 3, maximumFractionDigits: 3, useGrouping: false }
};

// How to render null/undefined values
const nullPolicy = { mode: 'dash' }; // 'empty' | 'dash' | 'text'

// Grouping configuration
const grouping = {
  enabled: true,
  // Group strictly by these two columns (case-insensitive SQL column names)
  keys: ['Origin', 'Destination'],
  // Hide the grouping columns from the table grid
  hideGroupColumnsInTable: true,
  // Always hide these columns from the grid (even if not grouping keys)
  // IMPORTANT: We no longer hide 'carrier' here (so it remains visible).
  alwaysHideColumnsInTable: ['Gr'],
  // Header text
  headerTemplate: ({ Origin, Destination, Count }) => {
    const o = String(Origin ?? '').trim() || '--';
    const d = String(Destination ?? '').trim() || '--';
    const n = Number(Count) || 0;
    if (o === '--' && d === '--') {
      return `No Group — ${n} ${n === 1 ? 'item' : 'items'}`;
    }
    return `${o} → ${d} — ${n} ${n === 1 ? 'item' : 'items'}`;
  }
};

/* =========================
   Dynamic column widths
   ========================= */

// Width presets (px). Keys can be SQL names OR friendly headers.
const columnWidthPx = {
  // IDs & references
  'ShipmentID': 80, 
  'SRRef': 120,
  'Client': 140, 
  'Consignment': 180, 
  'Carrier': 60,
  'Date': 90, 
  'Status': 80, 
  'Trip': 60, 
  'VehType': 60,
  // quantities
  'Qty': 50, 
  'Vol': 60, 
  'Wgt': 60, 
  'UOM': 55,
  // finance (AP+AR)
  'TVAmt': 100, 
  'SVAmt': 100,
  'ConCharge': 90, 
  'ConChCode': 90, 
  'ConAmt': 110, 
  'ConFaf': 90, 
  'ConFafAmt': 90, 
  'ConMod': 60,
  'CalcCharge': 90, 
  'CalcChCode': 90, 
  'CalcAmt': 110, 
  'CalcTotAmt': 110, 
  'CalcFaf': 90, 
  'CalcFafAmt': 90, 
  'CalcMod': 60,
  // misc
  'Domain': 75, 
  'Inv': 50, 
  'IsCon': 50, 
  'ConPass': 50,
  'CalcPass': 50,
  'Excl': 45
};

// Build a <colgroup> that matches the currently visible headers (or columns fallback).
function applyColGroup(table, headers, columns) {
  table.querySelectorAll('colgroup').forEach(cg => cg.remove());
  const cg = document.createElement('colgroup');

  headers.forEach((hdr, i) => {
    const colEl = document.createElement('col');

    // Prefer width by header caption; else by underlying column name
    const preferredKeys = [hdr, columns?.[i]].filter(Boolean);
    let width = null;
    for (const k of preferredKeys) {
      if (Object.prototype.hasOwnProperty.call(columnWidthPx, k)) {
        width = columnWidthPx[k];
        break;
      }
    }
    if (width != null) colEl.style.width = `${width}px`;

    // Stable class for potential CSS tweaks
    const cls = (columns?.[i] || hdr || '').replace(/[^\w]/g, '');
    if (cls) colEl.className = `col-${cls}`;

    cg.appendChild(colEl);
  });

  table.prepend(cg);
}

/* =========================
   Utility helpers
   ========================= */

function escapeHtml(s) {
  if (s == null) return '';
  return String(s).replace(/[&<>\"']/g, m => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]
  ));
}
function normalizeKey(s) { return String(s ?? '').toLowerCase(); }

function getAlignClass(columnNameOrHeader) {
  const target = normalizeKey(columnNameOrHeader);
  for (const k of Object.keys(columnAlignment)) {
    if (normalizeKey(k) === target) {
      const a = columnAlignment[k];
      if (a === 'right' || a === 'center') return `align-${a}`;
      return 'align-left';
    }
  }
  return 'align-left';
}
function getFormatRule(columnNameOrHeader) {
  const target = normalizeKey(columnNameOrHeader);
  for (const k of Object.keys(columnFormat)) {
    if (normalizeKey(k) === target) return columnFormat[k];
  }
  return null;
}
function isFiniteNum(v) { return typeof v === 'number' && Number.isFinite(v); }

/* =========================
   Formatters
   ========================= */
function formatCurrency(value, rule) {
  const {
    currency = 'NZD',
    locale = 'en-NZ',
    minimumFractionDigits = 2,
    maximumFractionDigits = 2,
    useGrouping = true,
    accounting = false
  } = rule || {};

  const num = Number(value);
  if (!Number.isFinite(num)) return escapeHtml(value);
  const intl = new Intl.NumberFormat(locale, {
    style: 'currency', currency, minimumFractionDigits, maximumFractionDigits, useGrouping
  });
  if (accounting && num < 0) {
    const absFormatted = intl.format(Math.abs(num));
    return `<span class="mono">(${absFormatted})</span>`;
  }
  return `<span class="mono">${intl.format(num)}</span>`;
}
function formatNumber(value, rule) {
  const {
    locale = undefined,
    minimumFractionDigits = 0,
    maximumFractionDigits = 20,
    useGrouping = false
  } = rule || {};
  const num = Number(value);
  if (!Number.isFinite(num)) return escapeHtml(value);
  const intl = new Intl.NumberFormat(locale, {
    minimumFractionDigits, maximumFractionDigits, useGrouping
  });
  return `<span class="mono">${intl.format(num)}</span>`;
}
function formatDateLike(value) {
  const s = String(value);
  if (/^\d{4}-\d{2}-\d{2}/.test(s)) return escapeHtml(s.slice(0, 10));
  return escapeHtml(s);
}
/**
 * Main cell formatter — uses column name/header to pick a rule.
 * Returns innerHTML for the <td>.
 */
function formatCellInnerHtml(value, columnNameOrHeader) {
  // Null handling
  if (value === null || value === undefined) {
    if (nullPolicy.mode === 'dash') return '<span class="null">–</span>';
    if (nullPolicy.mode === 'text') return '<span class="null">NULL</span>';
    return '';
  }
  // Rule-based formatting first
  const rule = getFormatRule(columnNameOrHeader);
  if (rule?.type === 'currency') return formatCurrency(value, rule);
  if (rule?.type === 'number')   return formatNumber(value, rule);

  // Heuristics
  if (isFiniteNum(value)) {
    const isInt = Number.isInteger(value);
    const opts = isInt
      ? { useGrouping: false, maximumFractionDigits: 0 }
      : { useGrouping: false, maximumFractionDigits: 20 };
    return `<span class="mono">${Number(value).toLocaleString(undefined, opts)}</span>`;
  }
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value)) return formatDateLike(value);
  return escapeHtml(value);
}

/* =========================
   Grouping helpers
   ========================= */

// For grouping keys: return null if any one is missing (so we fall back to ungrouped)
function findColumnIndexes(columns, names /* array */) {
  const idxs = [];
  const lower = columns.map(c => normalizeKey(c));
  for (const n of names) {
    const i = lower.indexOf(normalizeKey(n));
    if (i === -1) return null;
    idxs.push(i);
  }
  return idxs;
}
function findColumnIndexByName(headers, name) {
  const target = normalizeKey(name);
  const lower = headers.map(h => normalizeKey(h));
  return lower.indexOf(target);
}
// For "always hide" columns: return only the indexes that exist (ignore missing)
function findExistingColumnIndexes(columns, names /* array */) {
  const idxs = [];
  const lower = columns.map(c => normalizeKey(c));
  for (const n of names) {
    const i = lower.indexOf(normalizeKey(n));
    if (i !== -1) idxs.push(i);
  }
  return idxs;
}
function isExcludedRowByHeaders(row, headers) {
  const idx = findColumnIndexByName(headers, 'Excl');
  if (idx === -1) return false;
  const v = row[idx];
  return (typeof v === 'string' && v.trim().toLowerCase() === 'y');
}
function buildKeepIndexes(totalCount, removeIdxs /* array or Set */) {
  const removeSet = (removeIdxs instanceof Set) ? removeIdxs : new Set(removeIdxs);
  const keep = [];
  for (let i = 0; i < totalCount; i++) {
    if (!removeSet.has(i)) keep.push(i);
  }
  return keep;
}
function projectRow(row, keepIdxs) {
  return keepIdxs.map(i => row[i]);
}
function rowsGrouped(model, groupIdxs) {
  // Preserve first-seen group order
  const order = [];
  const map = new Map(); // key -> { keyVals:[], rows:[], keyObj:{} }
  for (const r of model.rows) {
    const keyVals = groupIdxs.map(i => r[i]);
    const key = JSON.stringify(keyVals); // safe for scalar keys
    let bucket = map.get(key);
    if (!bucket) {
      const keyObj = {};
      for (let k = 0; k < groupIdxs.length; k++) {
        const name = model.columns[groupIdxs[k]];
        keyObj[name] = keyVals[k];
      }
      bucket = { keyVals, rows: [], keyObj };
      map.set(key, bucket);
      order.push(key);
    }
    bucket.rows.push(r);
  }
  return order.map(k => map.get(k));
}

// Group subtotal fields (match against original model.columns)
const groupTotalFields = ['Qty', 'Vol', 'Wgt', 'TVAmt', 'SVAmt', 'ConAmt', 'CalcAmt', 'CalcTotAmt'];
function computeGroupTotals(rows, columns, fieldNames) {
  const lowerCols = columns.map(c => normalizeKey(c));
  const idxMap = Object.fromEntries(
    fieldNames.map(name => [name, lowerCols.indexOf(normalizeKey(name))])
  );
  const totals = Object.fromEntries(fieldNames.map(n => [n, 0]));
  for (const r of rows) {
    for (const name of fieldNames) {
      const i = idxMap[name];
      if (i !== -1) {
        const v = Number(r[i]);
        if (Number.isFinite(v)) totals[name] += v;
      }
    }
  }
  return totals;
}

/* =========================
   Rendering
   ========================= */
function renderTableHeader(thead, headers) {
  const headHtml = headers.map(h => {
    const al = getAlignClass(h);
    return `<th class="${al}" title="${escapeHtml(h)}">${escapeHtml(h)}</th>`;
  }).join('');
  thead.innerHTML = `<tr>${headHtml}</tr>`;
}
function renderTableBodyUngrouped(tbody, headers, rows) {
  const bodyHtml = rows.map(r => {
    const excluded = isExcludedRowByHeaders(r, headers);
    const cells = r.map((v, i) => {
      const hdr = headers[i];
      const al = getAlignClass(hdr);
      const raw = (v == null) ? '' : String(v);
      return `<td class="${al} col-${hdr.replace(/[^\w]/g,'')}" title="${escapeHtml(raw)}">${formatCellInnerHtml(v, hdr)}</td>`;
    }).join('');
    return `<tr class="${excluded ? 'excluded' : ''}">${cells}</tr>`;
  }).join('');
  tbody.innerHTML = bodyHtml;
}
function renderTableBodyGrouped(tbody, headers, model, groupIdxs, keepIdxs) {
  // Stable sort by Origin, then Destination (optional)
  const groups = rowsGrouped(model, groupIdxs).sort((a, b) => {
    const ao = String(a.keyObj.Origin ?? '').localeCompare(String(b.keyObj.Origin ?? ''));
    if (ao !== 0) return ao;
    return String(a.keyObj.Destination ?? '').localeCompare(String(b.keyObj.Destination ?? ''));
  });

  const colSpan = headers.length;
  const bodyHtml = groups.map(g => {
    // 1) Group header row
    const displayObj = {};
    for (const name of grouping.keys) {
      const exact = Object.prototype.hasOwnProperty.call(g.keyObj, name) ? name
                 : Object.keys(g.keyObj).find(k => normalizeKey(k) === normalizeKey(name));
      displayObj[name] = exact ? g.keyObj[exact] : null;
    }
    displayObj.Count = g.rows.length;
    const groupTitle = grouping.headerTemplate(displayObj);
    const headerRow = `<tr class="group-header"><td colspan="${colSpan}">${escapeHtml(groupTitle)}</td></tr>`;

    // 2) Project each row to currently visible columns (keepIdxs)
    const projectedRows = g.rows.map(r => projectRow(r, keepIdxs));
    const rowsHtml = projectedRows.map(r => {
      const tds = r.map((v, i) => {
        const hdr = headers[i]; // display header (after projection)
        const al  = getAlignClass(hdr);
        const raw = (v == null) ? '' : String(v);
        return `<td class="${al} col-${hdr.replace(/[^\w]/g,'')}" title="${escapeHtml(raw)}">${formatCellInnerHtml(v, hdr)}</td>`;
      }).join('');
      const excluded = isExcludedRowByHeaders(r, headers);
      return `<tr class="${excluded ? 'excluded' : ''}">${tds}</tr>`;
    }).join('');

    // 3) Compute totals for this group's raw rows using original column names
    const totals = computeGroupTotals(g.rows, model.columns, groupTotalFields);

    // 4) Build a footer row aligned to visible columns
    const footerTds = keepIdxs.map((origIdx, visIdx) => {
      const hdr = headers[visIdx];
      const colName = model.columns[origIdx]; // original SQL column name
      const al = getAlignClass(hdr);

      // If this visible column is totalled, render value
      if (groupTotalFields.some(n => normalizeKey(n) === normalizeKey(colName))) {
        const val = totals[colName];
        return `<td class="${al}" title="${escapeHtml(String(val))}">${formatCellInnerHtml(val, colName)}</td>`;
      }
      // Put the "Subtotal" label in the first visible column
      if (visIdx === 0) return `<td class="align-left" title="Subtotal for group">Subtotal</td>`;
      return `<td></td>`;
    }).join('');
    const footerRow = `<tr class="group-footer">${footerTds}</tr>`;

    // 5) Return header + rows + footer for this group
    return `${headerRow}${rowsHtml}${footerRow}`;
  }).join('');

  tbody.innerHTML = bodyHtml;
}

/**
 * Strict renderer: expects:
 * { columns: string[], headers?: string[], rows: any[][] }
 */
window.renderResultsStrict = (model) => {
  const status = document.getElementById('status');
  const thead  = document.getElementById('thead');
  const tbody  = document.getElementById('tbody');
  const table  = document.getElementById('tbl');

  // Guard
  if (!model || !Array.isArray(model.columns) || !Array.isArray(model.rows)) {
    thead.innerHTML = '';
    tbody.innerHTML = '<tr><td class="null">No data</td></tr>';
    if (status) status.textContent = '0 rows';
    if (window.chrome?.webview) window.chrome.webview.postMessage({ type: "render-complete" });
    return;
  }

  const cols = model.columns;
  const headersSource = (Array.isArray(model.headers) && model.headers.length === cols.length)
    ? model.headers : cols;

  let groupIdxs = null;
  if (grouping.enabled) groupIdxs = findColumnIndexes(cols, grouping.keys);

  // === NEW: Prefer showing Carrier, hide Client if both exist ===
  const lowerCols = cols.map(c => normalizeKey(c));
  const hasCarrier = lowerCols.includes('carrier');
  const hasClient  = lowerCols.includes('client');

  // Start with the "always hide" list (e.g., 'Gr') and add 'Client' if 'Carrier' is present.
  const dynamicAlwaysHide = new Set(grouping.alwaysHideColumnsInTable || []);
  if (hasCarrier && hasClient) {
    dynamicAlwaysHide.add('Client'); // case-insensitive handled later
  }

  if (grouping.enabled && groupIdxs && groupIdxs.length === grouping.keys.length) {
    // Determine extra columns to hide (e.g., dynamic Client), in addition to grouping columns
    const extraHideIdxs = findExistingColumnIndexes(cols, Array.from(dynamicAlwaysHide));
    const removeIdxs = new Set(grouping.hideGroupColumnsInTable ? [...groupIdxs, ...extraHideIdxs] : [...extraHideIdxs]);
    const keepIdxs = buildKeepIndexes(cols.length, removeIdxs);
    const headers = keepIdxs.map(i => headersSource[i]);

    // Dynamic colgroup
    applyColGroup(table, headers, keepIdxs.map(i => cols[i]));

    renderTableHeader(thead, headers);
    renderTableBodyGrouped(tbody, headers, model, groupIdxs, keepIdxs);
  } else {
    // Fallback: no grouping → still hide dynamic 'Client' (if Carrier exists) plus standard list
    const extraHideIdxs = findExistingColumnIndexes(cols, Array.from(dynamicAlwaysHide));
    const keepIdxs = buildKeepIndexes(cols.length, extraHideIdxs);
    const headers = keepIdxs.map(i => headersSource[i]);
    const projectedRows = model.rows.map(r => projectRow(r, keepIdxs));

    // Dynamic colgroup
    applyColGroup(table, headers, keepIdxs.map(i => cols[i]));

    renderTableHeader(thead, headers);
    renderTableBodyUngrouped(tbody, headers, projectedRows);
  }

  if (status) status.textContent = `${model.rows.length} rows`;
  if (window.chrome?.webview) {
    // Tell WinForms host we're completely done rendering
    window.chrome.webview.postMessage({ type: "render-complete" });
  }
};