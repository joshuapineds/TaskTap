(() => {
  const records = [
    { id: 'TSK-1019', type: 'Task', title: 'Laptop setup and software install', subject: 'Mika Santos', reason: 'Task completed and retained for audit reference', archivedBy: 'Admin Demo', archived: '2026-10-08', review: '2027-04-08', status: 'Archived', source: '[Admin] task-management.html?task=TSK-1019', description: 'Completed service listing retained for reference after the task was closed.' },
    { id: 'TSK-1015', type: 'Task', title: 'Grocery pickup from the public market', subject: 'Noel Garcia', reason: 'Cancelled by requester; listing closed', archivedBy: 'Admin Demo', archived: '2026-10-06', review: '2027-04-06', status: 'Archived', source: '[Admin] task-management.html?task=TSK-1015', description: 'Cancelled task kept for record keeping. No payment was processed by this demo.' },
    { id: 'TSK-1008', type: 'Task', title: 'Pet sitting for two afternoons', subject: 'Kyla Mendoza', reason: 'Listing expired without a confirmed helper', archivedBy: 'System', archived: '2026-10-03', review: '2027-04-03', status: 'Archived', source: '[Admin] task-management.html', description: 'Expired listing retained as a sample archive entry.' },
    { id: 'TSK-0996', type: 'Task', title: 'Replace kitchen faucet', subject: 'Enzo Ramirez', reason: 'Task completed; retention period started', archivedBy: 'Admin Demo', archived: '2026-09-29', review: '2027-03-29', status: 'Archived', source: '[Admin] task-management.html', description: 'Closed repair task retained for the standard review period.' },
    { id: 'TSK-0984', type: 'Task', title: 'Weekend math tutoring', subject: 'Lara Domingo', reason: 'Requester withdrew the listing', archivedBy: 'Admin Demo', archived: '2026-09-21', review: '2027-03-21', status: 'Archived', source: '[Admin] task-management.html', description: 'Withdrawn listing retained for reference.' },
    { id: 'USR-2479', type: 'User', title: 'Noel Garcia', subject: 'Member account', reason: 'Account closed at member request', archivedBy: 'Admin Demo', archived: '2026-10-05', review: '2027-04-05', status: 'Archived', source: '[Admin] user-detail.html?id=USR-2479', description: 'Member account record retained after a voluntary closure request.' },
    { id: 'USR-2475', type: 'User', title: 'Enzo Ramirez', subject: 'Member account', reason: 'Duplicate account confirmed during review', archivedBy: 'Admin Demo', archived: '2026-09-25', review: '2027-03-25', status: 'Archived', source: '[Admin] user-detail.html?id=USR-2475', description: 'Duplicate account record retained for moderation history.' },
    { id: 'USR-2476', type: 'User', title: 'Lara Domingo', subject: 'Member account', reason: 'Member requested account deactivation', archivedBy: 'Admin Demo', archived: '2026-09-18', review: '2027-03-18', status: 'Archived', source: '[Admin] user-detail.html?id=USR-2476', description: 'Deactivated member account retained for reference.' },
    { id: 'RPT-00980', type: 'Report', title: 'Reported task description', subject: 'Task listing report', reason: 'Resolved after review; no further action', archivedBy: 'Admin Demo', archived: '2026-10-04', review: '2027-04-04', status: 'Archived', source: '[Admin] report-management.html?id=RPT-00980', description: 'Closed report record. The report remains available in the report demo page.' },
    { id: 'RPT-00979', type: 'Report', title: 'Unresponsive task poster', subject: 'Member conduct report', reason: 'Case closed and documented', archivedBy: 'Admin Demo', archived: '2026-09-22', review: '2027-03-22', status: 'Archived', source: '[Admin] report-management.html?id=RPT-00979', description: 'Closed moderation case retained for review history.' }
  ];
  const el = id => document.getElementById(id);
  const rows = el('archiveRows');
  const state = { type: 'all', query: '', status: 'Archived', sort: 'recent', pendingRestore: null };
  const typeInfo = { Task: ['▤', 'task'], User: ['♙', 'user'], Report: ['⚑', 'report'] };
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[ch]));
  const formatDate = value => new Date(`${value}T00:00:00`).toLocaleDateString('en-PH', { day: 'numeric', month: 'short', year: 'numeric' });
  const visibleRecords = () => {
    const query = state.query.trim().toLowerCase();
    return records.filter(record => (state.type === 'all' || record.type === state.type)
      && (state.status === 'all' || record.status === state.status)
      && (!query || [record.id, record.title, record.subject, record.reason, record.archivedBy].join(' ').toLowerCase().includes(query)))
      .sort((a, b) => state.sort === 'oldest' ? a.archived.localeCompare(b.archived)
        : state.sort === 'type' ? a.type.localeCompare(b.type) || b.archived.localeCompare(a.archived)
          : state.sort === 'review-date' ? a.review.localeCompare(b.review)
            : b.archived.localeCompare(a.archived));
  };
  const archiveCount = type => records.filter(record => record.type === type && record.status === 'Archived').length;
  function renderSummary() {
    const active = records.filter(record => record.status === 'Archived');
    el('archivedTotal').textContent = active.length;
    el('archivedTasks').textContent = active.filter(item => item.type === 'Task').length;
    el('archivedMembers').textContent = active.filter(item => item.type === 'User').length;
    el('archivedReports').textContent = active.filter(item => item.type === 'Report').length;
    el('archiveTabAll').textContent = active.length;
    el('archiveTabTask').textContent = archiveCount('Task');
    el('archiveTabUser').textContent = archiveCount('User');
    el('archiveTabReport').textContent = archiveCount('Report');
  }
  function render() {
    const result = visibleRecords();
    rows.innerHTML = result.map(record => {
      const [symbol, cls] = typeInfo[record.type];
      const due = record.status === 'Archived' && record.review <= '2026-10-11';
      return `<tr><td><div class="archive-record"><span class="record-type-icon ${cls}">${symbol}</span><div><strong>${escapeHtml(record.title)}</strong><a href="${escapeHtml(record.source)}" data-source="${escapeHtml(record.id)}">${escapeHtml(record.id)} · ${escapeHtml(record.subject)}</a></div></div></td><td><span class="type-chip ${cls}">${record.type === 'User' ? 'Member' : record.type}</span></td><td class="reason-cell" title="${escapeHtml(record.reason)}">${escapeHtml(record.reason)}</td><td><span class="archive-by">${escapeHtml(record.archivedBy)}</span></td><td class="archive-date">${formatDate(record.archived)}</td><td class="review-date ${due ? 'due' : ''}">${formatDate(record.review)}${due ? ' · Due' : ''}</td><td><span class="archive-status ${record.status.toLowerCase()}">${record.status}</span></td><td><div class="row-actions"><button class="row-action" data-view="${escapeHtml(record.id)}">View</button>${record.status === 'Archived' ? `<button class="row-action restore" data-restore="${escapeHtml(record.id)}">Restore</button>` : '<button class="row-action" disabled>Restored</button>'}</div></td></tr>`;
    }).join('');
    el('archiveEmpty').hidden = result.length > 0;
    rows.closest('table').hidden = result.length === 0;
    const suffix = result.length === 1 ? 'record' : 'records';
    el('archiveResult').textContent = `Showing ${result.length} ${suffix}`;
    el('archiveFooterCount').textContent = `${result.length} sample ${suffix}`;
    el('clearArchiveFilters').hidden = !state.query && state.type === 'all' && state.status === 'Archived';
    renderSummary();
  }
  function openDetail(id) {
    const record = records.find(item => item.id === id);
    if (!record) return;
    const [symbol, cls] = typeInfo[record.type];
    el('archiveModalContent').innerHTML = `<p class="modal-kicker">ARCHIVE RECORD · ${escapeHtml(record.id)}</p><h2 id="archiveModalTitle">${escapeHtml(record.title)}</h2><p>${escapeHtml(record.description)}</p><div class="archive-detail-grid"><div class="archive-detail-item"><span>Record type</span><strong>${record.type === 'User' ? 'Member account' : `${record.type} record`}</strong></div><div class="archive-detail-item"><span>Archive status</span><strong>${escapeHtml(record.status)}</strong></div><div class="archive-detail-item"><span>Archive reason</span><strong>${escapeHtml(record.reason)}</strong></div><div class="archive-detail-item"><span>Archived / review by</span><strong>${formatDate(record.archived)} · ${formatDate(record.review)}</strong></div></div><div class="modal-actions"><a class="button secondary" href="${escapeHtml(record.source)}">Open source page</a><button class="button secondary" data-close-modal>Close</button>${record.status === 'Archived' ? `<button class="button primary" data-restore="${escapeHtml(record.id)}">Restore record</button>` : ''}</div>`;
    el('archiveModal').hidden = false;
  }
  function requestRestore(id) {
    const record = records.find(item => item.id === id && item.status === 'Archived');
    if (!record) return;
    state.pendingRestore = record;
    el('restoreTitle').textContent = `Restore ${record.type === 'User' ? 'member' : record.type.toLowerCase()} record?`;
    el('restoreCopy').textContent = `${record.title} (${record.id}) will be marked Restored in this archive demo. Its source record will not change.`;
    el('restoreModal').hidden = false;
  }
  function closeModals() { document.querySelectorAll('.modal-backdrop').forEach(modal => { modal.hidden = true; }); }
  document.querySelectorAll('.archive-tab').forEach(tab => tab.addEventListener('click', () => {
    state.type = tab.dataset.type;
    document.querySelectorAll('.archive-tab').forEach(item => { const selected = item === tab; item.classList.toggle('active', selected); item.setAttribute('aria-selected', selected); });
    render();
  }));
  el('archiveSearch').addEventListener('input', event => { state.query = event.target.value; render(); });
  el('archiveStatus').addEventListener('change', event => { state.status = event.target.value; render(); });
  el('archiveSort').addEventListener('change', event => { state.sort = event.target.value; render(); });
  el('includeRestored').addEventListener('change', event => { if (event.target.checked) el('archiveStatus').value = 'all'; else el('archiveStatus').value = 'Archived'; state.status = el('archiveStatus').value; render(); });
  el('clearArchiveFilters').addEventListener('click', () => {
    state.type = 'all'; state.query = ''; state.status = 'Archived'; state.sort = 'recent';
    el('archiveSearch').value = ''; el('archiveStatus').value = 'Archived'; el('archiveSort').value = 'recent'; el('includeRestored').checked = false;
    document.querySelector('.archive-tab[data-type="all"]').click();
  });
  rows.addEventListener('click', event => {
    const view = event.target.closest('[data-view]');
    const restore = event.target.closest('[data-restore]');
    if (view) openDetail(view.dataset.view);
    if (restore) requestRestore(restore.dataset.restore);
  });
  el('archiveModalContent').addEventListener('click', event => { const button = event.target.closest('[data-restore]'); if (button) requestRestore(button.dataset.restore); });
  document.querySelectorAll('[data-close-modal]').forEach(button => button.addEventListener('click', closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal => modal.addEventListener('click', event => { if (event.target === modal) closeModals(); }));
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeModals(); if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); el('archiveSearch').focus(); } });
  el('confirmRestore').addEventListener('click', () => {
    if (!state.pendingRestore) return;
    state.pendingRestore.status = 'Restored';
    const title = state.pendingRestore.title;
    state.pendingRestore = null;
    closeModals(); render();
    const toast = el('toast'); toast.textContent = `${title} marked restored in this demo.`; toast.classList.add('show');
    window.setTimeout(() => toast.classList.remove('show'), 2600);
  });
  el('exportArchives').addEventListener('click', () => {
    const lines = [['ID', 'Type', 'Title', 'Subject', 'Reason', 'Archived by', 'Archived date', 'Review date', 'Status'], ...visibleRecords().map(r => [r.id, r.type, r.title, r.subject, r.reason, r.archivedBy, r.archived, r.review, r.status])];
    const csv = lines.map(line => line.map(value => `"${String(value).replace(/"/g, '""')}"`).join(',')).join('\r\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob); const link = document.createElement('a'); link.href = url; link.download = 'tasktap-archive-demo.csv'; link.click(); URL.revokeObjectURL(url);
  });
  document.querySelectorAll('.future-link, [data-toast]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); const toast = el('toast'); toast.textContent = link.dataset.toast || `${link.dataset.page} is planned as a future admin page.`; toast.classList.add('show'); window.setTimeout(() => toast.classList.remove('show'), 2400); }));
  const sidebar = el('sidebar'); const scrim = el('sidebarScrim');
  el('mobileMenu')?.addEventListener('click', () => { sidebar.classList.add('open'); scrim.classList.add('visible'); });
  scrim?.addEventListener('click', () => { sidebar.classList.remove('open'); scrim.classList.remove('visible'); });
  el('helpButton')?.addEventListener('click', () => { const toast = el('toast'); toast.textContent = 'Archive status and restore actions are sample data for this frontend demo.'; toast.classList.add('show'); window.setTimeout(() => toast.classList.remove('show'), 2400); });
  render();
})();
