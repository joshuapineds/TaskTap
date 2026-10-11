(() => {
  const events = [
    { id: 'EVT-60219', date: '2026-10-11', time: '16:42:18', actor: 'Admin Demo', role: 'Administrator', module: 'Barangay Management', action: 'Updated service area details', object: 'BRY-0179 · Brgy. 179', outcome: 'Success', icon: '⌖', detail: 'The coordinator and coverage review date were updated in the sample barangay directory.', context: 'This event is a prewritten example. No live service area data was changed.' },
    { id: 'EVT-60218', date: '2026-10-11', time: '15:18:42', actor: 'Mia Santos', role: 'Moderator', module: 'Report Management', action: 'Resolved a member report', object: 'RPT-00980 · Report case', outcome: 'Success', icon: '✓', detail: 'A sample report was marked resolved after review. No member notification was sent.', context: 'Moderation outcomes shown here are illustrative and do not affect real accounts.' },
    { id: 'EVT-60217', date: '2026-10-11', time: '14:06:03', actor: 'Admin Demo', role: 'Administrator', module: 'User Management', action: 'Opened a member profile', object: 'USR-2479 · Noel Garcia', outcome: 'Success', icon: '♙', detail: 'The sample member record was viewed from the user directory.', context: 'Profile views are represented for interface demonstration only.' },
    { id: 'EVT-60216', date: '2026-10-11', time: '12:51:27', actor: 'System', role: 'Automated service', module: 'System', action: 'Daily archive review reminder', object: '2 archive records', outcome: 'Notice', icon: '◷', detail: 'A sample reminder indicates that two archive entries are approaching their review date.', context: 'This reminder is static demo content and is not connected to a notification service.' },
    { id: 'EVT-60215', date: '2026-10-11', time: '11:33:51', actor: 'Ramon Dela Cruz', role: 'Administrator', module: 'Payment Records', action: 'Flagged a payment entry for review', object: 'PAY-00482 · Member-entered record', outcome: 'Review', icon: '₱', detail: 'A sample payment record was flagged for follow-up because its entered amount needs confirmation.', context: 'TaskTap does not process, hold, or transfer money.' },
    { id: 'EVT-60214', date: '2026-10-11', time: '09:08:14', actor: 'Admin Demo', role: 'Administrator', module: 'Task Management', action: 'Archived a closed task', object: 'TSK-1019 · Laptop setup', outcome: 'Success', icon: '▤', detail: 'A closed sample task was moved into the archive register.', context: 'The task status in this log is sample content; this event does not modify another page.' },
    { id: 'EVT-60213', date: '2026-10-10', time: '17:29:08', actor: 'Mia Santos', role: 'Moderator', module: 'Report Management', action: 'Added an internal case note', object: 'RPT-00979 · Member report', outcome: 'Success', icon: '⚑', detail: 'A sample internal note was added to a report review.', context: 'Notes shown in this demo are not sent to members.' },
    { id: 'EVT-60212', date: '2026-10-10', time: '16:02:44', actor: 'Admin Demo', role: 'Administrator', module: 'Category Management', action: 'Edited a category description', object: 'CAT-004 · Delivery', outcome: 'Success', icon: '◈', detail: 'The sample Delivery category description was updated in the category prototype.', context: 'Category changes remain limited to their demo page.' },
    { id: 'EVT-60211', date: '2026-10-10', time: '13:44:32', actor: 'Ramon Dela Cruz', role: 'Administrator', module: 'Archives', action: 'Reviewed an archived member record', object: 'USR-2475 · Member archive', outcome: 'Review', icon: '▣', detail: 'A sample member archive entry was opened for a retention review.', context: 'No account was restored or modified.' },
    { id: 'EVT-60210', date: '2026-10-09', time: '16:27:10', actor: 'Admin Demo', role: 'Administrator', module: 'Barangay Management', action: 'Viewed coverage insights', object: 'Caloocan City · Coverage view', outcome: 'Success', icon: '⌖', detail: 'The illustrative service-area coverage overview was opened.', context: 'Coverage visualizations do not represent official boundaries or live availability.' },
    { id: 'EVT-60209', date: '2026-10-09', time: '15:13:39', actor: 'System', role: 'Automated service', module: 'System', action: 'Sample data refresh completed', object: 'Admin prototype', outcome: 'Notice', icon: '↻', detail: 'A sample system event indicates that the prototype display data was refreshed.', context: 'This is a static example, not an actual background process.' },
    { id: 'EVT-60208', date: '2026-10-09', time: '11:05:23', actor: 'Mia Santos', role: 'Moderator', module: 'User Management', action: 'Reviewed a verification submission', object: 'USR-2486 · Member verification', outcome: 'Review', icon: '◉', detail: 'A sample verification submission was marked for manual review.', context: 'No identity documents are stored or reviewed by this frontend.' },
    { id: 'EVT-60207', date: '2026-10-08', time: '18:31:57', actor: 'Admin Demo', role: 'Administrator', module: 'Task Management', action: 'Viewed reported task details', object: 'TSK-1015 · Delivery task', outcome: 'Success', icon: '▤', detail: 'An administrator opened a sample task with an attached report indicator.', context: 'No task owner was contacted.' },
    { id: 'EVT-60206', date: '2026-10-08', time: '14:20:11', actor: 'Ramon Dela Cruz', role: 'Administrator', module: 'Payment Records', action: 'Exported the filtered record list', object: 'Payment records · CSV', outcome: 'Success', icon: '↓', detail: 'A sample export event for member-entered payment information.', context: 'Exports in this prototype are generated locally in the browser.' },
    { id: 'EVT-60205', date: '2026-10-07', time: '16:48:06', actor: 'Admin Demo', role: 'Administrator', module: 'Category Management', action: 'Added a sample category', object: 'CAT-008 · Home support', outcome: 'Success', icon: '◈', detail: 'A sample service category was added to the category page.', context: 'This prewritten event does not add a new category in the current session.' },
    { id: 'EVT-60204', date: '2026-10-07', time: '13:11:49', actor: 'System', role: 'Automated service', module: 'Dashboard', action: 'Daily summary prepared', object: 'Community overview', outcome: 'Notice', icon: '▦', detail: 'A sample dashboard summary event for the admin overview.', context: 'Figures on the dashboard are illustrative frontend values.' },
    { id: 'EVT-60203', date: '2026-10-06', time: '17:03:22', actor: 'Mia Santos', role: 'Moderator', module: 'Report Management', action: 'Dismissed a duplicate report', object: 'RPT-00976 · Duplicate case', outcome: 'Success', icon: '⚑', detail: 'A sample duplicate report was dismissed during moderation review.', context: 'No real moderation case was created or dismissed.' },
    { id: 'EVT-60202', date: '2026-10-06', time: '10:49:37', actor: 'Admin Demo', role: 'Administrator', module: 'User Management', action: 'Exported the member directory', object: 'User list · CSV', outcome: 'Success', icon: '↓', detail: 'A sample export event for the member management directory.', context: 'CSV export actions operate only on sample browser data.' },
    { id: 'EVT-60201', date: '2026-10-05', time: '09:25:04', actor: 'Ramon Dela Cruz', role: 'Administrator', module: 'Dashboard', action: 'Opened the admin dashboard', object: 'A-01 · Admin overview', outcome: 'Success', icon: '▦', detail: 'The administrator viewed the TaskTap dashboard prototype.', context: 'This is the oldest event included in this sample register.' }
  ];
  const el = id => document.getElementById(id);
  const state = { query: '', date: '7', module: 'all', outcome: 'all' };
  const outcomeMeta = { Success: ['Success', 'success'], Review: ['Needs review', 'review'], Notice: ['Notice', 'notice'] };
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const formatDate = date => new Date(`${date}T00:00:00`).toLocaleDateString('en-PH', { day: 'numeric', month: 'short', year: 'numeric' });
  function dateMatches(event) {
    if (state.date === 'all') return true;
    if (state.date === 'today') return event.date === '2026-10-11';
    const days = Number(state.date);
    const age = (new Date('2026-10-11T00:00:00') - new Date(`${event.date}T00:00:00`)) / 86400000;
    return age >= 0 && age < days;
  }
  function filteredEvents() {
    const q = state.query.trim().toLowerCase();
    return events.filter(event => dateMatches(event)
      && (state.module === 'all' || event.module === state.module)
      && (state.outcome === 'all' || event.outcome === state.outcome)
      && (!q || [event.id, event.actor, event.role, event.module, event.action, event.object, event.outcome].join(' ').toLowerCase().includes(q)))
      .sort((a, b) => `${b.date}T${b.time}`.localeCompare(`${a.date}T${a.time}`));
  }
  function renderStats() {
    const today = events.filter(event => event.date === '2026-10-11');
    el('statToday').textContent = today.length;
    el('statSuccess').textContent = events.filter(event => event.outcome === 'Success').length;
    el('statReview').textContent = events.filter(event => event.outcome === 'Review').length;
    el('statActors').textContent = new Set(events.map(event => event.actor)).size;
    el('tabAll').textContent = events.length;
    el('tabSuccess').textContent = events.filter(event => event.outcome === 'Success').length;
    el('tabReview').textContent = events.filter(event => event.outcome === 'Review').length;
  }
  function renderChart() {
    const days = Array.from({ length: 7 }, (_, index) => {
      const date = new Date(2026, 9, 5 + index);
      const iso = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
      return { iso, label: date.toLocaleDateString('en-PH', { weekday: 'short' }), count: events.filter(event => event.date === iso).length };
    });
    const max = Math.max(1, ...days.map(day => day.count));
    el('activityChart').innerHTML = days.map(day => `<div class="chart-day ${day.iso === '2026-10-11' ? 'today' : ''}" title="${day.label}: ${day.count} events"><div class="chart-bar-track"><span class="chart-value">${day.count}</span><i class="chart-bar" style="height:${Math.max(6, (day.count / max) * 100)}%"></i></div><span class="chart-day-label">${day.label}</span></div>`).join('');
    el('chartTotal').textContent = `${days.reduce((sum, day) => sum + day.count, 0)} events this week`;
    const counts = new Map(); events.forEach(event => counts.set(event.module, (counts.get(event.module) || 0) + 1));
    const sorted = [...counts.entries()].sort((a, b) => b[1] - a[1]).slice(0, 5); const maxModule = Math.max(1, sorted[0]?.[1] || 1);
    el('moduleBars').innerHTML = sorted.map(([module, count]) => `<div class="module-row"><span>${escapeHtml(module)}</span><span class="module-track"><i style="width:${count / maxModule * 100}%"></i></span><strong>${count}</strong></div>`).join('');
  }
  function renderRows() {
    const list = filteredEvents();
    el('logRows').innerHTML = list.map(event => {
      const [label, cls] = outcomeMeta[event.outcome];
      const initials = event.actor === 'System' ? 'SY' : event.actor.split(' ').map(part => part[0]).slice(0, 2).join('').toUpperCase();
      return `<tr><td><div class="event-cell"><span class="event-icon ${cls}">${escapeHtml(event.icon)}</span><div class="event-copy"><strong>${escapeHtml(event.action)}</strong><small>${escapeHtml(event.id)} · ${escapeHtml(event.object)}</small></div></div></td><td><div class="actor-cell"><span class="actor-avatar ${event.actor === 'System' ? 'system' : ''}">${escapeHtml(initials)}</span><span class="actor-name"><strong>${escapeHtml(event.actor)}</strong><small>${escapeHtml(event.role)}</small></span></div></td><td><span class="module-chip">${escapeHtml(event.module)}</span></td><td class="event-time">${formatDate(event.date)}<small>${escapeHtml(event.time)} PHT</small></td><td><span class="outcome-chip ${cls}">${label}</span></td><td><button class="event-open" data-event="${escapeHtml(event.id)}">Details</button></td></tr>`;
    }).join('');
    el('logEmpty').hidden = list.length > 0;
    el('logRows').closest('table').hidden = list.length === 0;
    el('logResults').textContent = `Showing ${list.length} ${list.length === 1 ? 'sample event' : 'sample events'}`;
    el('logFooterCount').textContent = `Showing ${list.length} of ${events.length} events`;
    const filtered = state.query || state.date !== '7' || state.module !== 'all' || state.outcome !== 'all';
    el('clearLogFilters').hidden = !filtered;
  }
  function render() { renderStats(); renderChart(); renderRows(); }
  function syncQuickTabs() {
    document.querySelectorAll('.log-tab').forEach(tab => {
      const active = tab.dataset.outcome === state.outcome;
      tab.classList.toggle('active', active); tab.setAttribute('aria-selected', String(active));
    });
  }
  function openEvent(id) {
    const event = events.find(item => item.id === id); if (!event) return;
    const [label, cls] = outcomeMeta[event.outcome];
    el('eventModalContent').innerHTML = `<p class="event-kicker">ACTIVITY EVENT · ${escapeHtml(event.id)}</p><h2 id="eventModalTitle">${escapeHtml(event.action)}</h2><p>${escapeHtml(event.detail)}</p><div class="event-detail-grid"><div class="event-detail"><span>Actor</span><strong>${escapeHtml(event.actor)} · ${escapeHtml(event.role)}</strong></div><div class="event-detail"><span>Section</span><strong>${escapeHtml(event.module)}</strong></div><div class="event-detail"><span>Related record</span><strong>${escapeHtml(event.object)}</strong></div><div class="event-detail"><span>When</span><strong>${formatDate(event.date)} · ${escapeHtml(event.time)} PHT</strong></div><div class="event-detail"><span>Outcome</span><strong class="outcome-chip ${cls}">${label}</strong></div><div class="event-detail"><span>Event ID</span><strong>${escapeHtml(event.id)}</strong></div></div><p class="event-context">${escapeHtml(event.context)}</p><div class="modal-actions"><button class="button secondary" data-close-modal>Close details</button></div>`;
    el('eventModal').hidden = false;
  }
  function clearFilters() {
    state.query = ''; state.date = '7'; state.module = 'all'; state.outcome = 'all';
    el('logSearch').value = ''; el('dateFilter').value = '7'; el('moduleFilter').value = 'all'; el('outcomeFilter').value = 'all';
    syncQuickTabs(); renderRows();
  }
  function closeModal() { el('eventModal').hidden = true; }
  function toast(message) {
    const box = el('toast'); box.textContent = message; box.classList.add('show');
    window.clearTimeout(toast.timer); box.timer = window.setTimeout(() => box.classList.remove('show'), 2400);
  }
  el('logSearch').addEventListener('input', event => { state.query = event.target.value; renderRows(); });
  el('dateFilter').addEventListener('change', event => { state.date = event.target.value; renderRows(); });
  el('moduleFilter').addEventListener('change', event => { state.module = event.target.value; renderRows(); });
  el('outcomeFilter').addEventListener('change', event => { state.outcome = event.target.value; syncQuickTabs(); renderRows(); });
  document.querySelectorAll('.log-tab').forEach(tab => tab.addEventListener('click', () => {
    state.outcome = tab.dataset.outcome; el('outcomeFilter').value = state.outcome; syncQuickTabs(); renderRows();
  }));
  el('clearLogFilters').addEventListener('click', clearFilters);
  el('logRows').addEventListener('click', event => { const button = event.target.closest('[data-event]'); if (button) openEvent(button.dataset.event); });
  el('eventModalContent').addEventListener('click', event => { if (event.target.closest('[data-close-modal]')) closeModal(); });
  document.querySelectorAll('[data-close-modal]').forEach(button => button.addEventListener('click', closeModal));
  el('eventModal').addEventListener('click', event => { if (event.target === el('eventModal')) closeModal(); });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') closeModal();
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); el('logSearch').focus(); }
  });
  el('exportLog').addEventListener('click', () => {
    const data = [['Event ID', 'Date', 'Time PHT', 'Actor', 'Role', 'Section', 'Action', 'Related record', 'Outcome'], ...filteredEvents().map(event => [event.id, event.date, event.time, event.actor, event.role, event.module, event.action, event.object, event.outcome])];
    const csv = data.map(row => row.map(value => `"${String(value).replace(/"/g, '""')}"`).join(',')).join('\r\n');
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a'); link.href = url; link.download = 'tasktap-activity-log-demo.csv'; link.click(); URL.revokeObjectURL(url);
    toast('Sample activity log exported as a CSV file.');
  });
  el('refreshLog').addEventListener('click', () => { render(); toast('Sample activity log refreshed.'); });
  el('viewAllModules').addEventListener('click', event => { event.preventDefault(); el('moduleFilter').value = 'all'; state.module = 'all'; renderRows(); el('logTable').scrollIntoView({ behavior: 'smooth', block: 'start' }); });
  document.querySelectorAll('.future-link, [data-toast]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); toast(link.dataset.toast || `${link.dataset.page} is planned as a future admin page.`); }));
  el('helpButton')?.addEventListener('click', () => toast('These are read-only sample events, not live audit records.'));
  el('notificationButton')?.addEventListener('click', () => toast('No new activity log notices in this demo.'));
  const sidebar = el('sidebar'); const scrim = el('sidebarScrim');
  el('mobileMenu')?.addEventListener('click', () => { sidebar.classList.add('open'); scrim.classList.add('open'); });
  scrim?.addEventListener('click', () => { sidebar.classList.remove('open'); scrim.classList.remove('open'); });
  render();
})();
