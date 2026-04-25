class SupportTicket {
  final int id;
  final String subject;
  final String description;
  final String priority;
  final int status;
  final String statusName;
  final String? adminNotes;
  final List<String> imageUrls;
  final DateTime createdAt;
  final DateTime? updatedAt;

  SupportTicket({
    required this.id,
    required this.subject,
    required this.description,
    required this.priority,
    required this.status,
    required this.statusName,
    this.adminNotes,
    this.imageUrls = const [],
    required this.createdAt,
    this.updatedAt,
  });

  factory SupportTicket.fromJson(Map<String, dynamic> json) {
    final status = json['status'] as int? ?? 0;
    
    // statusName could be a String or int in the response
    String statusName;
    if (json['statusName'] is String) {
      statusName = json['statusName'] as String;
    } else if (json['statusName'] is int) {
      statusName = _getStatusName(json['statusName'] as int);
    } else {
      statusName = _getStatusName(status);
    }
    
    // priority could be a String or int in the response
    String priority;
    if (json['priority'] is String) {
      priority = json['priority'] as String;
    } else if (json['priority'] is int) {
      priority = _getPriorityName(json['priority'] as int);
    } else {
      priority = 'Normal';
    }
    
    return SupportTicket(
      id: json['id'] as int? ?? 0,
      subject: json['subject'] as String? ?? '',
      description: json['description'] as String? ?? '',
      priority: priority,
      status: status,
      statusName: statusName,
      adminNotes: json['adminNotes'] as String?,
      imageUrls: (json['imageUrls'] as List<dynamic>?)
              ?.map((url) => url as String)
              .toList() ??
          [],
      createdAt: json['createdAt'] != null 
          ? DateTime.parse(json['createdAt'] as String) 
          : DateTime.now(),
      updatedAt: json['updatedAt'] != null 
          ? DateTime.parse(json['updatedAt'] as String) 
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'subject': subject,
      'description': description,
      'priority': priority,
      'status': status,
      'statusName': statusName,
      'adminNotes': adminNotes,
      'imageUrls': imageUrls,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }

  static String _getStatusName(int status) {
    switch (status) {
      case 1:
        return 'Open';
      case 2:
        return 'In Progress';
      case 3:
        return 'Resolved';
      case 4:
        return 'Closed';
      default:
        return 'Unknown';
    }
  }
  
  static String _getPriorityName(int priority) {
    switch (priority) {
      case 1:
        return 'Low';
      case 2:
        return 'Normal';
      case 3:
        return 'High';
      case 4:
        return 'Urgent';
      default:
        return 'Normal';
    }
  }
}

class CreateSupportTicketRequest {
  final String subject;
  final String description;
  final String priority;

  CreateSupportTicketRequest({
    required this.subject,
    required this.description,
    this.priority = 'Normal',
  });

  Map<String, dynamic> toJson() {
    return {
      'subject': subject,
      'description': description,
      'priority': priority,
    };
  }
}
