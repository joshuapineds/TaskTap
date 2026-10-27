namespace TaskTap.Data;

/// <summary>Idempotent MySQL schema. Mirrors database/schema.sql; applied automatically at startup.</summary>
internal static class Schema
{
    public static readonly string[] Statements =
    [
        """
        CREATE TABLE IF NOT EXISTS users (
          id INT AUTO_INCREMENT PRIMARY KEY,
          first_name VARCHAR(60) NOT NULL,
          last_name VARCHAR(60) NOT NULL,
          email VARCHAR(190) NOT NULL,
          phone VARCHAR(40) NOT NULL DEFAULT '',
          password_hash VARCHAR(255) NOT NULL,
          role VARCHAR(20) NOT NULL DEFAULT 'Standard User',
          admin_role VARCHAR(30) NOT NULL DEFAULT '',
          barangay VARCHAR(20) NOT NULL DEFAULT '',
          status VARCHAR(20) NOT NULL DEFAULT 'Active',
          verified TINYINT(1) NOT NULL DEFAULT 0,
          verification_requested TINYINT(1) NOT NULL DEFAULT 0,
          verification_requested_at VARCHAR(40) NOT NULL DEFAULT '',
          verification_note VARCHAR(500) NOT NULL DEFAULT '',
          skills VARCHAR(300) NOT NULL DEFAULT '',
          bio VARCHAR(240) NOT NULL DEFAULT '',
          created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          last_active DATETIME NULL,
          UNIQUE KEY uq_users_email (email),
          KEY ix_users_phone (phone)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS categories (
          id VARCHAR(20) PRIMARY KEY,
          name VARCHAR(60) NOT NULL,
          icon VARCHAR(16) NOT NULL DEFAULT '',
          color_argb INT NOT NULL DEFAULT 0,
          accent_argb INT NOT NULL DEFAULT 0,
          active TINYINT(1) NOT NULL DEFAULT 1,
          sort_order INT NOT NULL DEFAULT 0
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS barangays (
          id VARCHAR(20) PRIMARY KEY,
          name VARCHAR(60) NOT NULL,
          city VARCHAR(80) NOT NULL DEFAULT '',
          latitude DOUBLE NULL,
          longitude DOUBLE NULL,
          active TINYINT(1) NOT NULL DEFAULT 1
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS tasks (
          id INT AUTO_INCREMENT PRIMARY KEY,
          poster_id INT NOT NULL,
          title VARCHAR(160) NOT NULL,
          category VARCHAR(60) NOT NULL,
          price INT NOT NULL DEFAULT 0,
          barangay VARCHAR(20) NOT NULL DEFAULT '',
          deadline DATE NOT NULL,
          icon VARCHAR(16) NOT NULL DEFAULT '',
          description TEXT NOT NULL,
          requirements TEXT NULL,
          task_type VARCHAR(40) NOT NULL DEFAULT '',
          payment_method VARCHAR(40) NOT NULL DEFAULT '',
          budget_type VARCHAR(40) NOT NULL DEFAULT '',
          duration VARCHAR(40) NOT NULL DEFAULT '',
          schedule_notes TEXT NULL,
          start_time VARCHAR(12) NOT NULL DEFAULT '',
          icon_back_argb INT NOT NULL DEFAULT 0,
          map_x INT NOT NULL DEFAULT 50,
          map_y INT NOT NULL DEFAULT 50,
          latitude DOUBLE NULL,
          longitude DOUBLE NULL,
          status VARCHAR(20) NOT NULL DEFAULT 'Active',
          featured TINYINT(1) NOT NULL DEFAULT 0,
          cancel_reason VARCHAR(500) NOT NULL DEFAULT '',
          closed_at DATETIME NULL,
          created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          KEY ix_tasks_poster (poster_id),
          KEY ix_tasks_status (status),
          CONSTRAINT fk_tasks_poster FOREIGN KEY (poster_id) REFERENCES users(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS task_requests (
          id INT AUTO_INCREMENT PRIMARY KEY,
          task_id INT NOT NULL,
          doer_id INT NOT NULL,
          message TEXT NULL,
          attachment_name VARCHAR(200) NULL,
          attachment_size INT NULL,
          attachment LONGBLOB NULL,
          status VARCHAR(20) NOT NULL DEFAULT 'pending',
          shortlisted TINYINT(1) NOT NULL DEFAULT 0,
          created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          UNIQUE KEY uq_request (task_id, doer_id),
          KEY ix_requests_doer (doer_id),
          CONSTRAINT fk_requests_task FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
          CONSTRAINT fk_requests_doer FOREIGN KEY (doer_id) REFERENCES users(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS saved_tasks (
          user_id INT NOT NULL,
          task_id INT NOT NULL,
          saved_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          PRIMARY KEY (user_id, task_id),
          CONSTRAINT fk_saved_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
          CONSTRAINT fk_saved_task FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS payments (
          id INT AUTO_INCREMENT PRIMARY KEY,
          reference VARCHAR(30) NOT NULL,
          owner_id INT NULL,
          task_title VARCHAR(160) NOT NULL,
          method VARCHAR(40) NOT NULL,
          status VARCHAR(20) NOT NULL DEFAULT 'Pending',
          amount DECIMAL(12,2) NOT NULL DEFAULT 0,
          paid_on DATE NOT NULL,
          category VARCHAR(60) NOT NULL DEFAULT '',
          barangay VARCHAR(20) NOT NULL DEFAULT '',
          person VARCHAR(140) NOT NULL DEFAULT '',
          description VARCHAR(500) NOT NULL DEFAULT '',
          UNIQUE KEY uq_payment_reference (reference),
          KEY ix_payments_owner (owner_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS notifications (
          id INT AUTO_INCREMENT PRIMARY KEY,
          user_id INT NOT NULL,
          title VARCHAR(160) NOT NULL,
          body VARCHAR(500) NOT NULL DEFAULT '',
          kind VARCHAR(20) NOT NULL DEFAULT 'tasks',
          action_label VARCHAR(60) NOT NULL DEFAULT '',
          created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          read_at DATETIME NULL,
          KEY ix_notifications_user (user_id, created_at),
          CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS reports (
          id VARCHAR(20) PRIMARY KEY,
          reporter_name VARCHAR(140) NOT NULL,
          target_type VARCHAR(20) NOT NULL,
          target_label VARCHAR(200) NOT NULL,
          reason VARCHAR(160) NOT NULL,
          severity VARCHAR(20) NOT NULL DEFAULT 'Medium',
          status VARCHAR(20) NOT NULL DEFAULT 'Open',
          created_at VARCHAR(40) NOT NULL,
          description TEXT NULL,
          resolution VARCHAR(500) NOT NULL DEFAULT '',
          created_ts DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS disputes (
          id VARCHAR(20) PRIMARY KEY,
          task_title VARCHAR(160) NOT NULL,
          poster_name VARCHAR(140) NOT NULL,
          doer_name VARCHAR(140) NOT NULL,
          amount INT NOT NULL DEFAULT 0,
          reason VARCHAR(500) NOT NULL,
          status VARCHAR(20) NOT NULL DEFAULT 'Open',
          opened_at VARCHAR(40) NOT NULL,
          resolution VARCHAR(500) NOT NULL DEFAULT '',
          created_ts DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS activity_log (
          id VARCHAR(20) PRIMARY KEY,
          actor VARCHAR(140) NOT NULL,
          action VARCHAR(120) NOT NULL,
          target VARCHAR(200) NOT NULL DEFAULT '',
          category VARCHAR(40) NOT NULL DEFAULT '',
          detail VARCHAR(500) NOT NULL DEFAULT '',
          created_at DATETIME NOT NULL,
          KEY ix_activity_created (created_at)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS support_tickets (
          id VARCHAR(20) PRIMARY KEY,
          name VARCHAR(140) NOT NULL,
          email VARCHAR(190) NOT NULL,
          category VARCHAR(60) NOT NULL,
          subject VARCHAR(200) NOT NULL,
          message TEXT NOT NULL,
          status VARCHAR(20) NOT NULL DEFAULT 'Open',
          created_at VARCHAR(40) NOT NULL,
          reply TEXT NULL,
          created_ts DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS announcements (
          id VARCHAR(20) PRIMARY KEY,
          title VARCHAR(200) NOT NULL,
          body TEXT NOT NULL,
          audience VARCHAR(60) NOT NULL DEFAULT 'All members',
          active TINYINT(1) NOT NULL DEFAULT 1,
          published_at VARCHAR(40) NOT NULL,
          author VARCHAR(140) NOT NULL DEFAULT '',
          created_ts DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS app_settings (
          skey VARCHAR(60) PRIMARY KEY,
          svalue VARCHAR(500) NOT NULL DEFAULT ''
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS role_permissions (
          role VARCHAR(30) NOT NULL,
          permission VARCHAR(40) NOT NULL,
          PRIMARY KEY (role, permission)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS reviews (
          id INT AUTO_INCREMENT PRIMARY KEY,
          reviewer_id INT NOT NULL,
          reviewee_id INT NOT NULL,
          task_title VARCHAR(160) NOT NULL,
          rating TINYINT NOT NULL,
          title VARCHAR(70) NOT NULL,
          body VARCHAR(500) NOT NULL,
          created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
          UNIQUE KEY uq_review (reviewer_id, reviewee_id, task_title),
          KEY ix_reviews_reviewee (reviewee_id),
          CONSTRAINT fk_reviews_reviewer FOREIGN KEY (reviewer_id) REFERENCES users(id) ON DELETE CASCADE,
          CONSTRAINT fk_reviews_reviewee FOREIGN KEY (reviewee_id) REFERENCES users(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """,
        """
        CREATE TABLE IF NOT EXISTS notification_preferences (
          user_id INT NOT NULL,
          preference_key VARCHAR(40) NOT NULL,
          enabled TINYINT(1) NOT NULL DEFAULT 1,
          PRIMARY KEY (user_id, preference_key),
          CONSTRAINT fk_notification_preferences_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        """
    ];
}
