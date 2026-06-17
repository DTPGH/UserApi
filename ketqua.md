# Kết quả

> **Migration đầu tiên và cập nhật database**

![This is an alt text.](/wwwroot/result_image/table_users.png "This is a sample image.")

> **API Tổng quát**

![alt text](/wwwroot/result_image/api_all.png)

> **API lấy danh sách**
1. Lấy danh sách User chưa bị xóa mềm.

![alt text](/wwwroot/result_image/api_getall_1.png)
![alt text](/wwwroot/result_image/api_getall_2.png)
![alt text](/wwwroot/result_image/api_getall_3.png)


2. API lấy danh sách có phân trang bằng pageIndex = 2 và pageSize = 2.

![alt text](wwwroot/result_image/api_getall_paging.png)

3. API lấy danh sách có sắp xếp theo Id, Name hoặc CreatedAt.

    3.1.Sắp xếp giảm dần theo Id
    ![alt text](wwwroot/result_image/api_getall_sort_1.png)
    ![alt text](wwwroot/result_image/api_getall_sort_2.png)
    
    3.2.Sắp xếp giảm dần theo Name
    ![alt text](wwwroot/result_image/api_getall_sort_3.png)
    ![alt text](wwwroot/result_image/api_getall_sort_4.png)
    
    3.3.Sắp xếp giảm đần theo CreatedAt 
    ![alt text](wwwroot/result_image/api_getall_sort_5.png)
    ![alt text](wwwroot/result_image/api_getall_sort_6.png)
    
4. API lấy danh sách có tìm kiếm theo Name. 
    ![alt text](wwwroot/result_image/api_getall_searchingbyname.png)


> **API lấy chi tiết**
    ![alt text](wwwroot/result_image/api_getuserbyid.png)

> **API thêm mới**
    ![alt text](wwwroot/result_image/api_createanewuser.png)

> **API cập nhật**
- Cập nhật user có id = 6
    ![alt text](wwwroot/result_image/api_updateuserbyid.png)

> **API xóa mềm**
- Xóa mềm user với id = 6
    ![alt text](wwwroot/result_image/api_softdeleteuser.png)


